# -*- coding: UTF-8 -*-

# %%
from sklearnex import patch_sklearn, unpatch_sklearn, config_context
patch_sklearn()
import time
import numpy as np
from sklearn.ensemble import RandomForestClassifier
from sklearn.decomposition import PCA, IncrementalPCA
import warnings
import math
from copy import deepcopy
from functools import partial


warnings.filterwarnings("ignore")

# Train an SPE classifier
from self_paced_ensemble import SelfPacedEnsembleClassifier
from self_paced_ensemble.canonical_ensemble import *
from self_paced_ensemble.utils import *
from Basic import FLDBServer
from Basic import server
from group_divider import initial, group_fixed_length_divider
import argparse
from collections import namedtuple
import cleanlab
from multiprocessing import Pool

CFG = namedtuple("CFG", ["class_change_strategy", "class_ratio_strategy", "class_ratio"])
DATABASE = namedtuple("DATABASE", ["host", "user", "password", "database"])
SERVER = namedtuple("SERVER", ["host", "port", "buffer_len"])

METHODS = ["SPEnsemble", "SMOTEBoost", "SMOTEBagging", "RUSBoost", "UnderBagging", "Cascade"]
RANDOM_STATE = 42

# Parse arguments

method = 'CleanLab'
# method = 'LocalOutlierFactor'

database_args = DATABASE("localhost", "sa", "Temp123456", "SoftwareFaultLocalization")  # 本地ip地址
# database_args = DATABASE("xx.xxx.xx.xx:1433", "sa", "Temp123456", "SoftwareFaultLocalization")  # 客户端ip地址
# server_args = SERVER("xx.xxx.xx.xx", 12223, 40960)  # 服务器ip地址
server_args = SERVER("127.0.0.1", 12223, 40960)  # 本地ip

def parse():
    """Parse system arguments."""
    parser = argparse.ArgumentParser(
        description='Self-paced Ensemble',
        usage='run_example.py --method <method> --n_estimators <integer> --runs <integer>'
    )
    parser.add_argument('--method', type=str, default='SPEnsemble',
                        choices=METHODS + ['all'], help='Name of ensmeble method')
    parser.add_argument('--n_estimators', type=int, default=10, help='Number of base estimators')
    parser.add_argument('--runs', type=int, default=10, help='Number of independent runs')
    return parser.parse_args()


def init_model(method, base_estimator, n_estimators):
    """return a model specified by 'method'."""
    if method == 'CleanLab':
        model = SelfPacedEnsembleClassifier(base_estimator=base_estimator, n_estimators=n_estimators, random_state=RANDOM_STATE)
    else:
        raise ValueError(f'Do not support method {method}. Only accept \
            \'SPEnsemble\', \'SMOTEBoost\', \'SMOTEBagging\', \'RUSBoost\', \
            \'UnderBagging\', \'Cascade\'.')
    return model


def hardness_func(method, model, X, y_observe):
    """return hardness of each instance."""

    if method == 'CleanLab':
        remained, new_X_train, new_y_train = reduced_same_keep_one(X, y_observe)
        hardness = []
        # 多数类用例数量需大于等于五个（cv_n_folds=5）, 少数类数目大于一个（不包含等于）
        if np.sum(y_observe == 1) > 1:
            # region cleanlab find label issues
            cl = cleanlab.classification.CleanLearning(model, seed=RANDOM_STATE, find_label_issues_kwargs={'filter_by': 'both'})
            label_issues = cl.find_label_issues(new_X_train, new_y_train)
            is_label_issue = label_issues['is_label_issue'].values
            label_quality = label_issues['label_quality'].values
            # 创建约简后的分类难度
            new_hardness = 1.0 - label_quality
            for index in range(len(is_label_issue)):
                if is_label_issue[index]:  # and is_label_issue2[index]:
                    new_hardness[index] = 1
            # 根据约简后的分类难度寻找约简前的分类难度
            i = 0
            for index in range(len(X)):
                if remained[index]:
                    hardness.append(new_hardness[i])
                    i += 1
                else:
                    the_X = X[index]
                    for new_index in range(len(new_X_train)):
                        if (the_X == new_X_train[new_index]).all() and y_observe[index] == new_y_train[new_index]:
                            hardness.append(new_hardness[new_index])
            hardness = np.array(hardness)
            # endregion
    else:
        model.fit(X, y_observe)
        # hardness = np.absolute(y_observe - model.predict(X))
        hardness = np.absolute(y_observe - model.predict_proba(X)[:, 1])
    return hardness


def asvoid(arr):
    """
    Based on http://stackoverflow.com/a/16973510/190597 (Jaime, 2013-06)
    View the array as dtype np.void (bytes). The items along the last axis are
    viewed as one value. This allows comparisons to be performed on the entire row.
    """
    arr = np.ascontiguousarray(arr)
    if np.issubdtype(arr.dtype, np.floating):
        """ Care needs to be taken here since
        np.array([-0.]).view(np.void) != np.array([0.]).view(np.void)
        Adding 0. converts -0. to 0.
        """
        arr += 0.
    return arr.view(np.dtype((np.void, arr.dtype.itemsize * arr.shape[-1])))


def inNd(a, b, assume_unique=False):
    a = asvoid(a)
    b = asvoid(b)
    return np.in1d(a, b, assume_unique)
def reduced_same(X_train, y_train):
    X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X_train, y_train, np.arange(len(X_train)))
    mask = ~inNd(X_maj, X_min)
    new_X_maj = X_maj[mask]
    new_y_maj = y_maj[mask]
    new_X_train = np.concatenate((new_X_maj, X_min))
    new_y_train = np.concatenate((new_y_maj, y_min))
    return new_X_train, new_y_train


def reduced_same_keep_one(X_train, y_train):
    X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X_train, y_train, np.arange(len(X_train)))

    mask_maj = [True] * len(X_maj)
    for i in range(len(X_maj)):
        the_X = X_maj[i]
        if np.any(np.all(X_min==the_X, axis=1)) and np.any(np.all(X_maj[:i]==the_X, axis=1)):
            mask_maj[i] = False
    remain_X_maj = X_maj[mask_maj]

    # 将相同元素和A中不包含在B中的元素进行合并
    remain_y_maj = np.full(len(remain_X_maj), y_maj[0])
    remained_min = [True]*len(y_min)
    new_X_train = np.concatenate((remain_X_maj, X_min))
    new_y_train = np.concatenate((remain_y_maj, y_min))
    remained = np.concatenate((mask_maj, remained_min))
    return remained, new_X_train, new_y_train

def swap_elements(A, B, C, D):
    # 从A中移除索引对应的元素
    removed_A = np.delete(A, C, axis=0)
    # 从B中移除索引对应的元素
    removed_B = np.delete(B, D, axis=0)
    # 将A中的元素放入B的末尾
    new_B = np.concatenate((removed_B, A[C]))
    # 将B中的元素放入A的末尾
    new_A = np.concatenate((removed_A, B[D]))
    return new_A, new_B

def select_cases_to_change_class(X_train_original, y_train_original, select_suc_cases, select_fal_cases):
    X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X_train_original, y_train_original, np.arange(len(X_train_original)))
    new_X_maj, new_X_min = swap_elements(X_maj, X_min, select_suc_cases, select_fal_cases)
    new_y_maj = np.full(len(new_X_maj), y_maj[0])
    new_y_min = np.full(len(new_X_min), y_min[0])
    new_X_train = np.concatenate((new_X_maj, new_X_min))
    new_y_train = np.concatenate((new_y_maj, new_y_min))
    return new_X_train, new_y_train


def determine_pca_dimension(data, explained_variance_threshold):
    # 执行PCA
    pca = PCA()
    pca.fit(data)

    # 计算累计解释方差
    explained_variance_ratio = pca.explained_variance_ratio_
    cumulative_variance_ratio = np.cumsum(explained_variance_ratio)

    # 确定降维维度
    dimension = np.argmax(cumulative_variance_ratio >= explained_variance_threshold) + 1

    return dimension



def main():

    # 启动与C#通信的服务器端
    the_server = server.CommunicateServer(server_args)
    connect = the_server.get_connect()
    while True:

        # receive data stream. it won't accept data packet greater than 1024 bytes
        data = connect.recv(the_server.buf).decode('UTF-8')
        if (not data) or data == '<EOF>':
            # if data is received break
            break
        if data == "<ConnectionTestMessage>":
            connect.send(data.encode('UTF-8'))
        else:
            print("\nfrom connected user: " + str(data))
            (ID, class_change_strategy, start_itimes, class_ratio_strategy, class_ratio, dtimes) = data.split(',')
            cfg = CFG(class_change_strategy, class_ratio_strategy, class_ratio)
            # Load train data
            # 从数据库中读取原覆盖矩阵
            covDataBase = FLDBServer.CovMatrixSelect(database_args)
            X_train_original, y_train_original = covDataBase.read_cov_matrix_info(ID, "不变更用例", 0)
            covDataBase.close()

            # 对X_train进行降维 降维到开根后的维数
            time1 = time.time()
            # print("降维之前的维度:{}".format(len(X_train[0])))
            n_components = min(len(X_train_original) - 1, math.ceil(np.sqrt(len(X_train_original[0]))))
            # n_components = determine_pca_dimension(X_train_original, 0.99)
            reducer = PCA(n_components=n_components, random_state=RANDOM_STATE, svd_solver='arpack')
            X_train_reduced = reducer.fit_transform(X_train_original)
            # print("降维之后的维度:{}".format(len(X_train_new[0])))
            print("降维所用时间:{}".format(time.time() - time1))
            time3 = time.time()
            # for itimes in tqdm(range(int(start_itimes), int(dtimes))):
            #     run(itimes, ID, cfg, dtimes, class_change_strategy, X_train_reduced, X_train_original, y_train_original)
            with Pool(5) as p:
                partial_f = partial(run, ID=ID, cfg=cfg, dtimes=dtimes, class_change_strategy=class_change_strategy, X_train_reduced=X_train_reduced, X_train_original=X_train_original, y_train_original=y_train_original)
                i_values = range(int(start_itimes), int(dtimes))
                p.map(partial_f, i_values)
            print("分类+分组所用时间:{}".format(time.time() - time3))
            data = 'FINISHED'
            connect.send(data.encode('UTF-8'))


def run(itimes, ID, cfg,  dtimes, class_change_strategy, X_train_reduced, X_train_original, y_train_original):

    # 从数据库中读取变更用例的序号
    if class_change_strategy == '不变更用例':
        X_changed = deepcopy(X_train_reduced)
        X_train = deepcopy(X_train_original)
        y_observed = deepcopy(y_train_original)
        y_train = deepcopy(y_train_original)
        select_suc_cases = []
        select_fal_cases = []

    else:
        changeDataBase = FLDBServer.TestCaseChangeClass(database_args)
        select_suc_cases, select_fal_cases = changeDataBase.read_case_change_info(ID, cfg.class_change_strategy,
                                                                                  itimes)
        X_changed, y_observed = select_cases_to_change_class(X_train_reduced, y_train_original,
                                                             select_suc_cases, select_fal_cases)
        X_train, y_train = select_cases_to_change_class(X_train_original, y_train_original,
                                                        select_suc_cases, select_fal_cases)
        changeDataBase.close()

    suc_count = np.sum(y_train == 0)
    fal_count = np.sum(y_train == 1)
    ratio = suc_count / fal_count if suc_count >= fal_count else fal_count / suc_count
    n_estimators = min(100, max(10, math.ceil(ratio)))


    groups = []
    model = init_model(
        method=method,
        n_estimators=n_estimators,
        base_estimator=RandomForestClassifier(n_estimators=10, random_state=RANDOM_STATE)
        # base_estimator=DecisionTreeClassifier(random_state=RANDOM_STATE)
    )

    time2 = time.time()
    # 用分类器进行分类，评估分类难度
    hardness = hardness_func(method, model, X_changed, y_observed)
    print("分类器所用时间:{}".format(time.time() - time2))

    # 拆分
    group = group_fixed_length_divider(hardness, X_train, y_train, int(itimes))
    groups.extend(group)
    group_data_base = FLDBServer.TestCaseDivClass(database_args)
    # 将拆分结果写入数据库
    group_data_base.insert_test_case_div_info(ID, cfg, itimes, dtimes, groups)



if __name__ == '__main__':
    main()

# %%
