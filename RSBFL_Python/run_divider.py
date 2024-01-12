"""
In this python script we provided an example of how to use our
implementation of ensemble methods to perform classification.
Usage:
```
python run_example.py --method=SPEnsemble --n_estimators=10 --runs=10
```
or with shortopts:
```
python run_example.py -m SPEnsemble -n 10 -r 10
```
run arguments:
    -m / --methods: string
    |   Specify which method were used to build the ensemble classifier.
    |   support: 'SPEnsemble', 'SMOTEBoost', 'SMOTEBagging', 'RUSBoost', 'UnderBagging', 'Cascade'
    -n / --n_estimators: integer
    |   Specify how much base estimators were used in the ensemble.
    -r / --runs: integer
    |   Specify the number of independent runs (to obtain mean and std)
"""

# -*- coding: UTF-8 -*-

# %%
from sklearnex import patch_sklearn, unpatch_sklearn, config_context
patch_sklearn()
import cupy as cp
import time
import numpy as np
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier
from sklearn.ensemble import IsolationForest
from sklearn.neighbors import LocalOutlierFactor
from sklearn.svm import OneClassSVM
from sklearn.decomposition import PCA, IncrementalPCA
from sklearn.model_selection import cross_val_predict
from sklearn.metrics import accuracy_score
import warnings
import math
import socket
from copy import deepcopy
from functools import partial


# import xgboost

warnings.filterwarnings("ignore")

# Train an SPE classifier
from self_paced_ensemble import SelfPacedEnsembleClassifier
from self_paced_ensemble.canonical_ensemble import *
from self_paced_ensemble.utils import *
from Basic import FLDBServer
from Basic import server
from group_divider import group_divider, group_max_cross_divider, group_max_divider, group_unsample_cross_divider, group_cross_divider, group_passed_divider, \
    group_unsampled, initial, group_cluster_divider, group_cluster2_divider, \
    group_unsample_divider, group_clean, group_enlarge_divider, group_failed_divider, group_divider_enlarge, group_max_ddu_divider, group_max_uniqueness_divider, group_max_parity_divider, \
    group_minimal_clean, group_failed_ddu_divider, group_failed_uniqueness_divider, group_one_uniqueness_divider, group_failed_uniqueness_difference_divider, group_max_uniqueness_difference_divider, group_fixed_uniqueness_divider, group_fixed_divider, group_fixed_length_divider, group_fix_divider, group_failed_uniqueness_try, group_enlarge
import argparse
from tqdm import trange, tqdm
from collections import namedtuple
import cleanlab
from multiprocessing import Pool



CFG = namedtuple("CFG", ["class_change_strategy", "class_ratio_strategy", "class_ratio"])
DATABASE = namedtuple("DATABASE", ["host", "user", "password", "database"])
SERVER = namedtuple("SERVER", ["host", "port", "buffer_len"])

METHODS = ["SPEnsemble", "SMOTEBoost", "SMOTEBagging", "RUSBoost", "UnderBagging", "Cascade"]
RANDOM_STATE = 42

# Parse arguments
# n_estimators = 10     # 训练器迭代次数
method = 'CleanLab'
# method = 'LocalOutlierFactor'

database_args = DATABASE("localhost", "sa", "Temp123456", "SoftwareFaultLocalization")  # 本地ip地址
# database_args = DATABASE("10.134.99.149:1433", "sa", "Temp123456", "SoftwareFaultLocalization")  # 客户端ip地址
# server_args = SERVER("10.134.99.77", 12223, 40960)  # 服务器ip地址
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
    if method == 'SPEnsemble':
        model = SelfPacedEnsembleClassifier(base_estimator=base_estimator, n_estimators=n_estimators)
    elif method == 'SMOTEBoost':
        model = SMOTEBoostClassifier(base_estimator=base_estimator, n_estimators=n_estimators)
    elif method == 'SMOTEBagging':
        model = SMOTEBaggingClassifier(base_estimator=base_estimator, n_estimators=n_estimators)
    elif method == 'RUSBoost':
        model = RUSBoostClassifier(base_estimator=base_estimator, n_estimators=n_estimators)
    elif method == 'UnderBagging':
        model = UnderBaggingClassifier(base_estimator=base_estimator, n_estimators=n_estimators)
    elif method == 'Cascade':
        model = BalanceCascadeClassifier(base_estimator=base_estimator, n_estimators=n_estimators)
    elif method == 'IsolationForest':
        model = IsolationForest(n_estimators=n_estimators)
    elif method == 'LocalOutlierFactor':
        model = LocalOutlierFactor()
    elif method == 'OneClassSVM':
        model = OneClassSVM(kernel='linear', gamma='auto')
    elif method == 'CleanLab':
        model = SelfPacedEnsembleClassifier(base_estimator=base_estimator, n_estimators=n_estimators, random_state=RANDOM_STATE)
        # model = base_estimator
        # model = cleanlab.classification.LearningWithNoisyLabels(
        #     clf=_clf)

    else:
        raise ValueError(f'Do not support method {method}. Only accept \
            \'SPEnsemble\', \'SMOTEBoost\', \'SMOTEBagging\', \'RUSBoost\', \
            \'UnderBagging\', \'Cascade\'.')
    return model


def hardness_func(method, model, X, y_observe):
    def scale(data):
        _range = (-data + 2) / 4
        return _range

    def normalization(data):
        """ 将范围压缩到0,1之间 """
        _range = np.zeros(len(data))
        _range[data < 0] = 1
        return _range

    def adjust(data):
        if np.max(data) == np.min(data):
            return np.ones(len(data))
        else:
            return data

    if method == 'IsolationForest' or method == 'OneClassSVM':
        X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X, y_observe, np.arange(len(X)))
        model2 = deepcopy(model)
        model.fit(X_maj)
        model2.fit(X_min)
        X_maj_self_confident = adjust(model.predict(X_maj))
        X_min_self_confident = adjust(model2.predict(X_min))
        X_maj_trans_confident = model2.predict(X_maj)
        X_min_trans_confident = model.predict(X_min)
        hardness_maj = scale(X_maj_self_confident - X_maj_trans_confident)
        hardness_min = scale(X_min_self_confident - X_min_trans_confident)
        # hardness_maj = normalization(model.decision_function(X_maj) - model2.decision_function(X_maj))
        # hardness_min = normalization(model2.decision_function(X_min) - model.decision_function(X_min))
        hardness = np.concatenate((hardness_maj, hardness_min))

    elif method == 'LocalOutlierFactor':
        X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X, y_observe, np.arange(len(X)))
        # maj_neighbor = min(20, math.ceil(len(X_maj) * 0.5))
        # min_neighbor = min(20, math.ceil(len(X_min) * 0.5))
        model = LocalOutlierFactor()
        model2 = LocalOutlierFactor()
        # 新建模型
        new_model = LocalOutlierFactor(novelty=True)
        new_model.fit(X_maj)
        new_model2 = LocalOutlierFactor(novelty=True)
        new_model2.fit(X_min)
        hardness_maj = scale(model.fit_predict(X_maj) - new_model2.predict(X_maj))
        hardness_min = scale(model2.fit_predict(X_min) - new_model.predict(X_min))
        # model.fit_predict(X_maj)
        # model2.fit_predict(X_min)
        # hardness_maj = normalization(model.negative_outlier_factor_ - new_model2.score_samples(X_maj))
        # hardness_min = normalization(model2.negative_outlier_factor_ - new_model.score_samples(X_min))
        hardness = np.concatenate((hardness_maj, hardness_min))
    elif method == 'CleanLab':
        # X = np.array([[1, 2, 3, 4, 5], [1, 2, 3, 4, 5], [-1, -3, -5, -7, -9], [1, 3, 5, 7, 9], [1, 3, 5, 7, 9] , [1, 3, 5, 7, 9], [-1, -1, -1, -1, -1], [1, 2, 3, 4, 5], [1, 2, 3, 4, 5], [-1, -2, -3, 4, 5], [-1, -2, -3, 4, 5], [-1, -2, -3, -4, -5]])
        # y_observe = np.array([1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0])
        remained, new_X_train, new_y_train = reduced_same_keep_one(X, y_observe)
        # remained, new_X_train, new_y_train = [True] * len(X), X, y_observe
        hardness = []
        # 该方法存在限制：多数类用例数量需大于等于五个（cv_n_folds=5）, 少数类数目大于一个（不包含等于）
        if np.sum(y_observe == 1) > 1:
            # with config_context(target_offload="gpu:0"):
            # region cross_validation + filter
            # num_crossval_folds = 10
            # pred_probs = cross_val_predict(
            #     model,
            #     X,
            #     y_observe,
            #     cv=num_crossval_folds,
            #     method="predict_proba",
            # )
            #
            # is_label_issue = cleanlab.filter.find_label_issues(
            #     y_observe,
            #     pred_probs,
            #     filter_by='both',
            # )

            # endregion

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

            # cl_2 = cleanlab.classification.CleanLearning(model, seed=RANDOM_STATE, find_label_issues_kwargs={'filter_by': 'confident_learning'})
            # label_issues2 = cl_2.find_label_issues(X, y_observe)
            # is_label_issue2 = label_issues2['is_label_issue'].values

            # for index in range(len(is_label_issue)):
            #     if is_label_issue[index]:  # and is_label_issue2[index]:
            #         hardness[index] = 1

            # for index in range(len(is_label_issue)):
            #     the_X = new_X_train[index]
            #     # the_hardness = 1.0 - label_quality[index]
            #     if is_label_issue[index]:  # and is_label_issue2[index]:
            #         for new_index in range(len(X)):
            #             if (the_X == X[new_index]).all() and y_observe[new_index] == new_y_train[index]:
            #                 hardness[new_index] = 1
            #     # else:
            #     #     for new_index in range(len(X)):
            #     #         if (the_X == X[new_index]).all() and y_observe[new_index] == new_y_train[index]:
            #     #             hardness[new_index] = the_hardness

            hardness = np.array(hardness)
            # endregion

            # psx = cleanlab.latent_estimation.estimate_cv_predicted_probabilities(
            #     X, y_observe, clf=model)
            # confident_joint, psx = cleanlab.latent_estimation.estimate_confident_joint_and_cv_pred_proba(
            #     X=X,
            #     s=y_observe,
            #     clf=model,  # default, you can use any classifier
            # )
            # label_error_indices = cleanlab.pruning.get_noise_indices(
            #     s=y_observe,
            #     psx=psx,
            #     prune_method='prune_by_noise_rate',
            #     sorted_index_method='normalized_margin',
            # )
            # label_error_indices = cleanlab.pruning.get_noise_indices(
            #     s=y_observe,
            #     psx=psx,
            #     prune_method='both',
            #     sorted_index_method='self_confidence',
            # )

            # clf = cleanlab.classification.LearningWithNoisyLabels(clf=model, prune_method='both').fit(X, y_observe)
            # hardness = np.absolute(y_observe - clf.predict_proba(X)[:, 1])

        # hardness = hardness.astype(np.int32)
    else:
        model.fit(X, y_observe)
        # hardness = np.absolute(y_observe - model.predict(X))
        hardness = np.absolute(y_observe - model.predict_proba(X)[:, 1])
    return hardness

def ideal_hardness(X, y_observe, select_suc_num, select_fal_num, rate):
    X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X, y_observe, np.arange(len(X)))
    new_select_fal_num = math.ceil(select_fal_num * rate)
    new_select_suc_num = math.ceil(select_suc_num * rate)
    hardness_maj = np.array([0] * (len(X_maj) - new_select_fal_num) + [1] * new_select_fal_num)
    hardness_min = np.array([0] * (len(X_min) - new_select_suc_num) + [1] * new_select_suc_num)
    hardness = np.concatenate((hardness_maj, hardness_min))
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
    # new_X_maj, index_maj = np.unique(X_maj, axis=0, return_index=True)
    # new_y_maj = np.array([y_maj[index] for index in index_maj])
    mask = ~inNd(X_maj, X_min)
    new_X_maj = X_maj[mask]
    new_y_maj = y_maj[mask]
    # new_X_min, index_min = np.unique(X_min, axis=0, return_index=True)
    # new_y_min = np.array([y_min[index] for index in index_min])
    new_X_train = np.concatenate((new_X_maj, X_min))
    new_y_train = np.concatenate((new_y_maj, y_min))
    return new_X_train, new_y_train


def reduced_same_keep_one(X_train, y_train):
    X_maj, X_min, y_maj, y_min, index_maj, index_min = initial(X_train, y_train, np.arange(len(X_train)))

    # new_X_maj, index_maj = np.unique(X_maj, axis=0, return_index=True)
    # new_y_maj = np.array([y_maj[index] for index in index_maj])

    # removed_maj = inNd(X_maj, X_min)
    # mask_maj = ~removed_maj
    # remain_X_maj = X_maj[mask_maj]
    # X_maj_unique = np.unique(X_maj[removed_maj], axis=0)

    mask_maj = [True] * len(X_maj)
    for i in range(len(X_maj)):
        the_X = X_maj[i]
        if np.any(np.all(X_min==the_X, axis=1)) and np.any(np.all(X_maj[:i]==the_X, axis=1)):
            mask_maj[i] = False
    remain_X_maj = X_maj[mask_maj]

    # 将相同元素和A中不包含在B中的元素进行合并
    remain_y_maj = np.full(len(remain_X_maj), y_maj[0])
    # new_X_min, index_min = np.unique(X_min, axis=0, return_index=True)
    # new_y_min = np.array([y_min[index] for index in index_min])
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
    # FLDBServer.delete_table_from_database(database_args, "随机试验统计结果表", "实验结果表", "随机变更用例类别_集成实验分组表", "测试用例分组表")
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
            # n_components = min(len(X_train_original) - 1, math.ceil(np.sqrt(len(X_train_original[0]))))
            # n_components = min(len(X_train_original) - 1, math.ceil(len(X_train_original[0]) / 10))
            n_components = determine_pca_dimension(X_train_original, 0.99)
            # n_components = min(len(X_train_original) - 1, math.ceil(np.sqrt(len(X_train_original[0]) * 10)))
            # n_components = len(X_train_original) - 1
            reducer = PCA(n_components=n_components, random_state=RANDOM_STATE, svd_solver='arpack')
            # reducer = umap.UMAP(n_components=n_components, random_state=RANDOM_STATE)
            # reducer = umap.UMAP(n_components=10, random_state=RANDOM_STATE)

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

    # X_train, y_train = np.array([[0, 1, 0], [1, 0, 1], [1, 1, 0], [0, 1, 1], [1, 1, 1], [1, 0, 0], [0, 0, 0]]), np.array([0, 0, 0, 0, 0, 1, 1])
    suc_count = np.sum(y_train == 0)
    fal_count = np.sum(y_train == 1)
    ratio = suc_count / fal_count if suc_count >= fal_count else fal_count / suc_count
    # sample_num = max(1, math.ceil(0.9 * covDataBase.fal_num))  # 每次抽样(分组)个数是少数类的一部分
    # n_sampling = min(100, max(10, math.floor(ratio)))       # 采样次数(分组数量)为不平衡率
    # n_estimators = max(10, math.ceil(ratio))
    n_estimators = min(100, max(10, math.ceil(ratio)))
    # n_estimators = 20
    # n_estimators = max(5, math.ceil(math.sqrt(ratio)))          # 采样次数为不平衡率的十分之一
    # n_estimators = 50

    # print('\nRunning method:\t\t{} - {} estimators in {} independent run(s) ...'.format(
    #     method, n_estimators, itimes))
    # if class_change_strategy == '不变更用例':
    #     select_suc_num, select_fal_num = 0, 0
    # else:
    #     changeDataBase = FLDBServer.TestCaseChangeClass(database_args)
    #     select_suc_num, select_fal_num = changeDataBase.read_case_change_info(ID, cfg.class_change_strategy,
    #                                                                           itimes)
    #     changeDataBase.close()
    # Train & Record

    groups = []
    model = init_model(
        method=method,
        n_estimators=n_estimators,
        base_estimator=RandomForestClassifier(n_estimators=10, random_state=RANDOM_STATE)
        # base_estimator=DecisionTreeClassifier(random_state=RANDOM_STATE)
    )

    time2 = time.time()
    # 用分类器进行分类，评估分类难度
    # hardness_zero = np.array([0]*len(X_train))
    # hardness_monitor = ideal_hardness(X_changed, y_observed, len(select_suc_cases), len(select_fal_cases), 1)
    hardness = hardness_func(method, model, X_changed, y_observed)
    print("分类器所用时间:{}".format(time.time() - time2))

    # group = group_clean(hardness, X_train, y_train)
    # group = group_failed_ddu_divider(hardness, X_train, y_train)
    # group = group_failed_uniqueness_difference_divider(hardness, X_train, y_train)
    # group = group_fixed_uniqueness_divider(hardness, X_train, y_train)
    # group = group_max_uniqueness_difference_divider(hardness, X_train, y_train)
    # group = group_failed_uniqueness_divider(hardness, X_train, y_train)
    # group = group_failed_uniqueness_try(hardness, X_train, y_train)
    # group = group_one_uniqueness_divider(hardness_monitor, X_train, y_train)
    # group = group_max_ddu_divider(hardness, X_train, y_train)
    # group = group_max_uniqueness_divider(hardness, X_train, y_train)
    # group = group_fixed_divider(hardness, X_train, y_train)
    group = group_fixed_length_divider(hardness, X_train, y_train, int(itimes))
    # group = group_unsample_cross_divider(hardness_zero, X_train, y_train)
    # group = group_cross_divider(hardness_zero, X_train, y_train)
    # group = group_divider_enlarge(hardness_zero, X_train, y_train)
    # group = group_failed_divider(hardness, X_train, y_train)
    # group = group_passed_divider(hardness, X_train, y_train)
    # group = group_divider_enlarge(hardness_zero, X_train, y_train)
    # group = group_cluster2_divider(hardness_zero, X_train, y_train)
    # group = group_unsampled(hardness_zero, X_train, y_train)
    # group = group_enlarge(hardness, X_train, y_train, itimes)


    groups.extend(group)
    time4 = time.time()
    group_data_base = FLDBServer.TestCaseDivClass(database_args)
    group_data_base.insert_test_case_div_info(ID, cfg, itimes, dtimes, groups)
    # print("\n写入数据库所用时间:{}".format(time.time() - time4))
    # send data to the client


if __name__ == '__main__':
    main()

# %%
