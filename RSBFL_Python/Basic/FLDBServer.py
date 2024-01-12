#######################
#
#  数据库算法 增删查改
#
#######################

import pymssql
from Basic.Gzip import gzip_decompress, gzip_compress
import numpy as np


def connectSql(database_args):
    return pymssql.connect(host=database_args.host, user=database_args.user, port='1433',
                           password=database_args.password, database=database_args.database, charset="utf8")


class CovMatrixSelect(object):

    def __init__(self, database_args):
        self.db = connectSql(database_args)
        self.suc_num = None
        self.fal_num = None

    def read_cov_matrix_info(self, ID, description, itimes):
        # 打开数据库连接
        cur = self.db.cursor()

        if description == '不变更用例':
            itimes = 0

        # 读取覆盖矩阵表
        sqlstr = "SELECT [成例数量],[失例数量],[成例矩阵],[失例矩阵] FROM [SoftwareFaultLocalization].[dbo].[覆盖矩阵表] " \
                 "WHERE ID = '{}' AND 用例选取策略描述 = '{}' AND 实验次数 = '{}'".format(int(ID), description, int(itimes))
        cur.execute(sqlstr)
        data_set = cur.fetchall()
        cur.close()

        # 未读取数据 or 数据为空
        if len(data_set) == 0 or len(data_set[0]) == 0:
            return None

        self.suc_num = int(str(data_set[0][0]))
        self.fal_num = int(str(data_set[0][1]))
        raw_suc_str = str(data_set[0][2])
        raw_fal_str = str(data_set[0][3])

        # Gzip 解压
        suc_str = gzip_decompress(raw_suc_str)
        fal_str = gzip_decompress(raw_fal_str)

        result = {}
        suc_list = []
        fal_list = []
        if suc_str != '':
            suc_runs = suc_str.split(';')[:-1]
            suc_list = [list(map(int, run)) for run in suc_runs]
        if fal_str != '':
            fal_runs = fal_str.split(';')[:-1]
            fal_list = [list(map(int, run)) for run in fal_runs]
        suc_array = np.array(suc_list)
        fal_array = np.array(fal_list)
        X_train = np.vstack((suc_array, fal_array))
        # 将标签进行了反转， 适应SPE算法"多数类标签为1"的要求
        y_train = np.hstack((np.zeros(self.suc_num, dtype=np.int32), np.ones(self.fal_num, dtype=np.int32)))
        return X_train, y_train

    def close(self):
        self.db.close()


class TestCaseDivClass(object):

    def __init__(self, database_args):
        self.db = connectSql(database_args)

    def insert_test_case_div_info(self, ID, cfg, itimes, dtimes, groups):
        # 打开数据库连接
        cur = self.db.cursor()

        # 若数据库中有相同key的数据，则更新
        temp = self.read_test_case_div_info(ID, cfg, itimes, dtimes)
        if temp != None:
            self.update_test_case_div_info(ID, cfg, itimes, dtimes, groups)
        else:
            num = len(groups)
            suc_str = ''
            fal_str = ''
            for i in range(num):
                # 多数类标签为0
                suc_case_ids = groups[i]['0']
                suc_str += ','.join(str(i) for i in suc_case_ids)
                suc_str += ';'

                fal_case_ids = groups[i]['1']
                fal_str += ','.join(str(i) for i in fal_case_ids)
                fal_str += ';'

            suc_str = gzip_compress(suc_str)
            fal_str = gzip_compress(fal_str)

            # 写入覆盖矩阵表
            sqlstr = "INSERT INTO 随机变更用例类别_集成实验分组表(ID, 变更用例选取策略描述, 变更用例实验次数, \
                     分组策略描述, 类别比例, 实验次数, 分组数量, 成例分组记录, 失例分组记录)" + "VALUES(" \
                     + "'{}','{}','{}','{}','{}','{}','{}','{}','{}')" \
                         .format(int(ID), cfg.class_change_strategy, int(itimes), cfg.class_ratio_strategy,
                                 float(cfg.class_ratio), int(dtimes), int(num), suc_str, fal_str)
            cur.execute(sqlstr)
            self.db.commit()

    def read_test_case_div_info(self, ID, cfg, itimes, dtimes):
        # 打开数据库连接
        cur = self.db.cursor()
        try:
            # 读取覆盖矩阵表
            sqlstr = "SELECT [分组数量],[成例分组记录],[失例分组记录] FROM 随机变更用例类别_集成实验分组表 \
                    WHERE ID='{}' AND 变更用例选取策略描述='{}' AND 变更用例实验次数='{}' \
                     AND 分组策略描述='{}' AND 类别比例='{}' AND 实验次数='{}'".format(int(ID), cfg.class_change_strategy, int(itimes),
                                                                         cfg.class_ratio_strategy, float(cfg.class_ratio), int(dtimes))
            cur.execute(sqlstr)
            data_set = cur.fetchall()
            cur.close()
        except:
            return None

        # 未读取数据 or 数据为空
        if len(data_set) == 0 or len(data_set[0]) == 0:
            return None

        # 分组参数
        num = data_set[0][0]
        suc_str = data_set[0][1]
        fal_str = data_set[0][2]

        result = {}
        if suc_str != '':
            suc_runs = suc_str.split(';')[:-1]
            suc_list = [list(map(int, run.split(','))) for run in suc_runs]
            result['passed'] = suc_list
        if fal_str != '':
            fal_runs = fal_str.split(';')[:-1]
            fal_list = [list(map(int, run.split(','))) for run in fal_runs]
            result['failed'] = fal_list

        return result

    def dele_test_case_div_info(self, ID, cfg, itimes, dtimes):
        # 打开数据库连接
        cur = self.db.cursor()

        try:
            sqlstr = "DELETE FROM 随机变更用例类别_集成实验分组表 \
                     WHERE ID='{}' AND 变更用例选取策略描述='{}' AND 变更用例实验次数='{}' \
                     AND 分组策略描述='{}' AND 类别比例='{}' AND 实验次数='{}'".format(int(ID), cfg.class_change_strategy,
                                                                         int(itimes),
                                                                         cfg.class_ratio_strategy,
                                                                         float(cfg.class_ratio),
                                                                         int(dtimes))
            cur.execute(sqlstr)
            self.db.commit()
        except Exception as e:
            pass

    def update_test_case_div_info(self, ID, cfg, itimes, dtimes, groups):
        cur = self.db.cursor()
        try:
            self.dele_test_case_div_info(ID, cfg, itimes, dtimes)
            self.insert_test_case_div_info(ID, cfg, itimes, dtimes, groups)
        except Exception as e:
            pass

    def close(self):
        self.db.close()


class TestCaseChangeClass(object):

    def __init__(self, database_args):
        self.db = connectSql(database_args)

    def read_case_change_info(self, ID, description, itimes):
        # 打开数据库连接
        cur = self.db.cursor()

        # 读取覆盖矩阵表
        sqlstr = "SELECT [成例选取记录],[失例选取记录] FROM [SoftwareFaultLocalization].[dbo].[随机变更用例类别表] " \
                 "WHERE ID = '{}' AND 用例选取策略描述 = '{}' AND 实验次数 = '{}'".format(int(ID), description, int(itimes))
        cur.execute(sqlstr)
        data_set = cur.fetchall()
        cur.close()

        # 未读取数据 or 数据为空
        if len(data_set) == 0 or len(data_set[0]) == 0:
            return None

        select_suc_str = data_set[0][0]
        select_fal_str = data_set[0][1]

        select_suc_list, select_fal_list = [], []
        if select_suc_str != '':
            select_suc_list = list(map(int, select_suc_str.split(',')))
        if select_fal_str != '':
            select_fal_list = list(map(int, select_fal_str.split(',')))

        return select_suc_list, select_fal_list

    def close(self):
        self.db.close()


def delete_table_from_database(database_args, *tables):
    db = connectSql(database_args)
    cur = db.cursor()
    for table in tables:
        try:
            sqlstr = "truncate table {}".format(table)
            cur.execute(sqlstr)
            db.commit()
        except Exception as e:
            pass




if __name__ == "__main__":
    from collections import namedtuple

    # 从数据库中读取
    DATABASE = namedtuple("DATABASE", ["host", "user", "password", "database"])
    database_args = DATABASE("localhost", "temp", "Temp123456", "SoftwareFaultLocalization")
    covDataBase = CovMatrixSelect(database_args)
    X_train, y_train = covDataBase.read_cov_matrix_info(4310, "变更用例0.1000", 2)
    covDataBase.close()

    # CFG = namedtuple("CFG", ["class_change_strategy", "class_ratio_strategy", "class_ratio"])
    # cfg = CFG("变更用例0.0100", "随机拆分", 1.0)
    # test_case_div_database = TestCaseChangeClass(database_args)
    # groups = [{'1':[1, 3, 5, 7], '0':[1, 2, 3, 4]},
    #            {'1':[2, 5, 7, 8], '0':[1, 2, 3, 4]}]
    # test_case_div_database.insert_test_case_div_info(1, cfg, 0, 1, groups)
    # test_case_div_database.close()
