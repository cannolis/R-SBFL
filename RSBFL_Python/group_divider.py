import sys

import numpy
import numpy as np
import math
import time
from collections import Counter
import random
from copy import deepcopy
from scipy.stats import norm, mstats

import scipy
from scipy.optimize import curve_fit
from sklearn.cluster import KMeans
from sklearn.mixture import GaussianMixture, BayesianGaussianMixture
# import hdbscan
from Basic import knee_locator
import matplotlib.pyplot as plt
from numpy.polynomial import Polynomial
from sklearn.linear_model import LinearRegression


def cloned(failed_remained_index, target_num):
    """ 克隆测试用例集到目标数量 """
    if len(failed_remained_index) >= target_num:
        return failed_remained_index
    else:
        # 计算扩增倍数和剩余的数量
        enlaged_times = target_num // len(failed_remained_index)
        remained_num = target_num % len(failed_remained_index)
        # 先扩充一定倍数
        failed_enlaged_index = np.repeat(failed_remained_index, enlaged_times)
        # 随机抽样剩余数量
        remained_index = np.random.choice(failed_remained_index, remained_num, replace=False)
        # 合并
        failed_enlaged_index = np.concatenate((failed_enlaged_index, remained_index))
        return failed_enlaged_index


# region  丢弃一些数据
# 3sigma法 数据需要服从正态分布 感觉我们的数据并不满足
def drop_use_3sigma(hardness, least_num, n=3):
    mean = np.mean(hardness)
    sigma = np.std(hardness)

    remove_idx = np.where(hardness - mean > n * sigma)
    hardness_index = np.arange(len(hardness))
    remain_index = np.delete(hardness_index, remove_idx)
    # 若剩余数量小于最少数量，则保留最小数量
    if len(remain_index) < least_num:
        # 从低到高排序
        hardness_index = np.argsort(hardness)
        remain_index = hardness_index[:least_num]
    return remain_index


# MAD法: media absolute deviation
def drop_use_MAD(hardness, least_num, n=2.5):
    median = np.median(hardness)  # 中位数
    deviations = abs(hardness - median)
    mad = np.median(deviations)

    remove_idx = np.where(hardness - median > n * mad)
    hardness_index = np.arange(len(hardness))
    remain_index = np.delete(hardness_index, remove_idx)
    # 若剩余数量小于最少数量，则保留最小数量
    if len(remain_index) < least_num:
        # 从低到高排序
        hardness_index = np.argsort(hardness)
        remain_index = hardness_index[:least_num]
    return remain_index


def drop_hard_data(hardness, hard_threshold, least_num):
    """ 扔掉分类难度在threshold及以上的数据 """
    remove_idx = np.where(hardness >= hard_threshold)
    hardness_index = np.arange(len(hardness))
    remain_index = np.delete(hardness_index, remove_idx)
    # 若剩余数量小于最少数量，则保留最小数量
    if len(remain_index) < least_num:
        # 从低到高排序
        hardness_index = np.argsort(hardness)
        remain_index = hardness_index[:least_num]

    return remain_index


def drop_some_data(hardness, least_num):
    # 扔掉分类难度较大的，留下least_num个数据
    remain_index = np.argsort(hardness)  # 从低到高排序
    # 丢弃10%的难分类数据
    remain_index = remain_index[:least_num]
    return remain_index


# endregion


def random_under_sampling(hardness, sample_num):
    """Private function used to perform random under-sampling."""
    idx = np.random.choice(len(hardness), sample_num, replace=False)
    x_index = np.arange(len(hardness))
    sampled_index = x_index[idx]

    return sampled_index


def ass_num(_len_bins, sample_num, _sample_rate):
    """ 小球分箱算法
    _len_bins: 每个箱子的容量
    sample_num: 总的小球数量
    _sample_rate: 分配给每个箱子的概率
    """
    len_bins = _len_bins.copy()
    sample_rate = _sample_rate.copy()
    n_sample_bins = sample_num * sample_rate / sample_rate.sum()
    temp_sample_bins = n_sample_bins.astype(int)
    if temp_sample_bins.sum() < sample_num:
        num = sample_num - temp_sample_bins.sum()
        prob_sample_bins = n_sample_bins - temp_sample_bins
        # 对未达到数量的重新抽取
        for i in range(num):
            select_index = random.choices(range(len(prob_sample_bins)), weights=prob_sample_bins)
            temp_sample_bins[select_index] += 1

    # 以盒中数量和暂定抽取数量中较少的为准
    n_sample_bins = deepcopy(temp_sample_bins)
    temp_sample_bins = np.array([min(len_bins[i_bins], temp_sample_bins[i_bins]) for i_bins in range(len(len_bins))])
    if temp_sample_bins.sum() < sample_num:
        # 更新抽取概率，若暂定数量不小于盒中数量，将该盒概率置为零
        for i_bins in range(len(len_bins)):
            sample_rate[i_bins] = sample_rate[i_bins] if temp_sample_bins[i_bins] < len_bins[i_bins] else 0
        sample_num = n_sample_bins.sum() - temp_sample_bins.sum()
        add_sample_bins = ass_num(len_bins - temp_sample_bins, sample_num, sample_rate)
        temp_sample_bins = temp_sample_bins + add_sample_bins
    return temp_sample_bins


def aug_list(the_list, num):
    new_list = deepcopy(the_list)
    for j in range(len(the_list)):
        new_list[j] = np.repeat(new_list[j], num, axis=0)
    return new_list


def bin_func(the_num, bin_num, base_num=12):
    if the_num > base_num:
        assert bin_num > 0
        min_num = the_num - base_num
        quotient = int(min_num / bin_num)
        remainder = min_num % bin_num
        if remainder > 0:
            bins = [quotient] * (bin_num - remainder) + [quotient + 1] * remainder
        elif remainder < 0:
            bins = [quotient - 1] * - remainder + [quotient] * (bin_num + remainder)
        else:
            bins = [quotient] * bin_num

        new_bins = [i for i in range(1, base_num + 1)]
        for index in range(len(bins)):
            if bins[index] != 0:
                new_bins.append(sum(bins[:index + 1]) + base_num)
    else:
        new_bins = [i for i in range(1, the_num + 1)]

    return new_bins


def under_sampling_base(hardness, sample_num, group_num):
    """Private function used to perform self-paced under-sampling."""

    all_sampled_indexes = []
    for i in range(group_num):
        all_sampled_indexes.append(random_under_sampling(hardness, sample_num))

    all_sampled_indexes = np.array(all_sampled_indexes)
    return all_sampled_indexes


# def under_sampling_base(hardness, sample_num):
#     """Private function used to perform self-paced under-sampling."""
#
#     # If hardness values are not distinguishable, perform random smapling
#     if hardness.max() == hardness.min():
#         all_sampled_indexes = random_under_sampling(hardness, sample_num)
#
#     # Else allocate majority samples into k hardness bins
#     else:
#
#         # region Abandon! 按难度分箱并按约定的采样率抽取
#         ranges = np.max(hardness) - np.min(hardness)
#         hardness = (hardness - np.min(hardness)) / ranges
#         # 设置步长
#         k_bins = 10
#         step = (hardness.max() - hardness.min()) / k_bins
#         ave_contributions = []
#         X_indexs = np.arange(len(hardness))
#         bins_indexs = []
#
#         for f_bins in range(k_bins):
#             idx = (
#                     (hardness >= f_bins * step + hardness.min()) &
#                     (hardness < (f_bins + 1) * step + hardness.min())
#             )
#             # Marginal samples with highest hardness value -> kth bin
#             if f_bins == (k_bins - 1):
#                 idx = idx | (hardness == hardness.max())
#             bins_indexs.append(X_indexs[idx])
#             ave_contributions.append(hardness[idx].mean())
#
#         len_bins = np.array([len(bins_indexs[i_bins]) for i_bins in range(len(bins_indexs))])
#         sample_rate = np.array([(1.001 - con) for con in ave_contributions])  # 给分类难度为1的bins留千分之一的采样率
#         sample_rate[np.isnan(sample_rate)] = 0
#
#         # Caculate Fault Localization sample number from each bin
#         sample_bins = ass_num(len_bins, sample_num, sample_rate)
#         # Perform Fault Localization self-paced under-sampling
#         sampled_indexes = []
#         for f_bins in range(k_bins):
#             if min(len_bins[f_bins], sample_bins[f_bins]) > 0:
#                 idx = np.random.choice(
#                     len_bins[f_bins],
#                     min(len_bins[f_bins], sample_bins[f_bins]),
#                     replace=False)
#                 sampled_indexes.append(bins_indexs[f_bins][idx])
#         if len(sampled_indexes) == 1:
#             all_sampled_indexes = sampled_indexes[0]
#         else:
#             all_sampled_indexes = np.concatenate(sampled_indexes, axis=0)
#         # endregion
#
#     return all_sampled_indexes


def initial(X, y, hardness):
    # 将X，y分解为 X_passed、X_failed和 y_passed、y_failed
    sorted_class_distr = sorted(Counter(y).items(), key=lambda d: d[1])
    # label_failed, label_passed = sorted_class_distr[0][0], sorted_class_distr[1][0]
    label_failed, label_passed = 1, 0
    passed_index, failed_index = (y == label_passed), (y == label_failed)
    X_passed, y_passed = X[passed_index], y[passed_index]
    X_failed, y_failed = X[failed_index], y[failed_index]
    hardness_passed, hardness_failed = hardness[passed_index], hardness[failed_index]

    return X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed


def concatenate(*array):
    array_list = list(array)
    temp_array_list = []
    for array in array_list:
        if len(array) != 0:
            temp_array_list.append(array)
    return np.concatenate(tuple(temp_array_list), axis=0)


def average_allot(n_divider_index, rdd_index):
    i_index = 0
    divide_num = len(n_divider_index)
    temp_n_divider_index = list(n_divider_index)
    while i_index < len(rdd_index):
        temp_n_divider_index[i_index % divide_num] = np.append(temp_n_divider_index[i_index % divide_num],
                                                               rdd_index[i_index])
        i_index += 1
    return temp_n_divider_index


def divide_and_abandon(bins_indexs, group_len):
    if len(bins_indexs) != 0:
        rdd_num = len(bins_indexs) % group_len
        group_num = len(bins_indexs) // group_len
        if group_num < 1:
            n_divide_index = np.array([bins_indexs])
            abandon_index = np.array([])
        else:
            divide_index = bins_indexs[:-rdd_num] if rdd_num != 0 else bins_indexs
            n_divide_index = np.array_split(divide_index, group_num)
            abandon_index = bins_indexs[-rdd_num:] if rdd_num != 0 else np.array([])
    else:
        n_divide_index = np.array([])
        abandon_index = np.array([])
    return n_divide_index, abandon_index


def divide_and_resample(bins_indexs, group_len, random_state=42):
    if len(bins_indexs) >= group_len:
        rdd_num = len(bins_indexs) % group_len
        group_num = len(bins_indexs) // group_len
        if group_num < 1:
            n_divide_index = np.array([bins_indexs])
        else:
            divide_index = bins_indexs[:-rdd_num] if rdd_num != 0 else bins_indexs
            n_divide_index = np.array_split(divide_index, group_num)
            if rdd_num != 0:
                np.random.seed(random_state)
                resample_index = np.concatenate(
                    (bins_indexs[-rdd_num:], np.random.choice(divide_index, size=group_len - rdd_num, replace=False)),
                    axis=0)
                n_divide_index = np.concatenate((n_divide_index, [resample_index]))
    elif len(bins_indexs) == 0:
        n_divide_index = np.array([])
    else:
        resample_index = np.concatenate(
            (bins_indexs, np.random.choice(bins_indexs, size=group_len - len(bins_indexs), replace=True)),
            axis=0)
        n_divide_index = [resample_index]
    return n_divide_index


def num2group(feet: int,
              heads: int,
              chicken_feet: int,
              rabbit_feet: int,
              chicken: str = "min",
              rabbit: str = "max"
              ) -> dict:
    """
    Chicken rabbit cages(ordinary).
    :param feet: The number of heads in total.
    :param heads: The number of heads in total.
    :param chicken_feet: Each chicken has a few feet.
    :param rabbit_feet: Each rabbit has a few feet.
    :param chicken: "chicken" 's name.
    :param rabbit: "rabbit" 's name.
    :return: results -> dict
    """

    if rabbit_feet < chicken_feet:
        raise ValueError("'chicken_feet' is greater than 'rabbit_feet'")
    results = {chicken: None, rabbit: int((feet - heads * chicken_feet) / (rabbit_feet - chicken_feet))}
    results[chicken] = heads - results[rabbit]
    if results[chicken] < 0:
        raise Exception(f"{chicken}'s num isn't positive number.")
    if results[rabbit] < 0:
        raise Exception(f"{rabbit}'s num isn't positive number.")
    return results


def divide_based_on_ratio(bin_indexs, group_num_ratio):
    indexs = deepcopy(bin_indexs)
    n_divide_index = []
    if len(indexs) != 0:
        group_num = len(group_num_ratio)
        bins = np.array([sys.maxsize] * group_num)
        passed_num = ass_num(bins, len(indexs), group_num_ratio)
        for i in range(group_num):
            chosed_index = np.random.choice(indexs, passed_num[i], replace=False)
            indexs = np.setdiff1d(indexs, chosed_index)
            n_divide_index.append(chosed_index)
        n_divide_index = np.array(n_divide_index)
    else:
        n_divide_index = np.array([])
    return n_divide_index


def distinguish_several_regions(hardness, bins_len=10):
    _hardness = deepcopy(hardness)
    hardness_index = np.arange(len(_hardness))
    # 拆分时丢弃多余数据
    # rdd_num = len(_hardness) % group_len
    # del_index = np.random.choice(hardness_index, rdd_num)
    # hardness_index = np.delete(hardness_index, del_index)
    # _hardness = np.delete(_hardness, del_index)

    # 设置步长
    k_bins = bins_len
    step = (_hardness.max() - _hardness.min()) / k_bins
    bins_index_under = []
    bins_index_upper = []
    for f_bins in range(k_bins):
        idx = (
                (_hardness >= f_bins * step + _hardness.min()) &
                (_hardness < (f_bins + 1) * step + _hardness.min())
        )
        # Marginal samples with highest hardness value -> kth bin
        if f_bins < k_bins - 1:
            bins_index_under.extend(hardness_index[idx])
        else:
            idx = idx | (_hardness == _hardness.max())
            bins_index_upper = hardness_index[idx]
    np.random.shuffle(bins_index_under)
    np.random.shuffle(bins_index_upper)
    return bins_index_under, bins_index_upper


def distinguish_two_regions(arr, confident_ratio=0.5):
    index_array = np.arange(len(arr))  # 创建索引数组
    sorted_indices = np.argsort(arr)  # 排序后的索引数组
    length = len(arr)  # 数组长度
    split_index = math.ceil(confident_ratio * length)  # 前%的位置索引

    # 获取前80%的索引数组
    first_80_indices = index_array[sorted_indices[:split_index]]

    # 获取后10%的索引数组
    last_20_indices = index_array[sorted_indices[split_index:]]

    np.random.shuffle(first_80_indices)
    np.random.shuffle(last_20_indices)

    return first_80_indices, last_20_indices


def distinguish_regions(hardness, group_len):
    _hardness = deepcopy(hardness)
    hardness_index = np.arange(len(_hardness))
    # 拆分时丢弃多余数据
    # rdd_num = len(_hardness) % group_len
    # del_index = np.random.choice(hardness_index, rdd_num)
    # hardness_index = np.delete(hardness_index, del_index)
    # _hardness = np.delete(_hardness, del_index)

    # 设置步长
    k_bins = 10
    step = (_hardness.max() - _hardness.min()) / k_bins
    bins_index_under = []
    bins_index_upper = []
    bins_index_top = []
    for f_bins in range(k_bins):
        idx = (
                (_hardness >= f_bins * step + _hardness.min()) &
                (_hardness < (f_bins + 1) * step + _hardness.min())
        )
        # Marginal samples with highest hardness value -> kth bin
        if f_bins < k_bins // 2:
            bins_index_under.extend(hardness_index[idx])
        elif f_bins < k_bins - 1:
            bins_index_upper.extend(hardness_index[idx])
        else:
            idx = idx | (_hardness == _hardness.max())
            bins_index_top = hardness_index[idx]
    np.random.shuffle(bins_index_under)
    np.random.shuffle(bins_index_upper)
    np.random.shuffle(bins_index_top)
    return bins_index_under, bins_index_upper, bins_index_top


def divide_nonuniform(hardness, group_len, random_state):
    bins_index_under, bins_index_upper, bins_index_top = distinguish_regions(hardness, group_len)
    bins_index_down = concatenate(bins_index_under, bins_index_upper)
    np.random.seed(random_state)
    np.random.shuffle(bins_index_down)
    # hardness_index = concatenate(bins_index_down, bins_index_top)
    hardness_index = concatenate(bins_index_top, bins_index_down)

    # 拆分
    n_divider_index, rdd_index = divide_and_abandon(hardness_index, group_len)

    # 拆分时多余(低难度)数据均分
    n_divider_index = n_divider_index[::-1]
    n_divider_index = average_allot(n_divider_index, rdd_index)
    return n_divider_index


def divide_gradient(hardness, group_len, random_state):
    # hardness_index = concatenate(bins_index_down, bins_index_top)
    # bins_index_under, bins_index_upper = distinguish_two_regions(hardness, 0.1)
    # # 拆分
    # n_divider_index_under = divide_and_resample(bins_index_under, group_len, random_state)
    # n_divider_index_upper = divide_and_resample(bins_index_upper, group_len, random_state)
    # n_divider_index = np.concatenate((n_divider_index_under, n_divider_index_upper), axis=0)

    hardness_index = [i for i in range(len(hardness))]
    hardness_index.sort(key=lambda x: hardness[x])
    n_divider_index = divide_and_resample(hardness_index, group_len, random_state)

    return n_divider_index


def divide_part_nonuniform(hardness, group_len, random_state):
    bins_index_under, bins_index_upper = distinguish_two_regions(hardness)
    np.random.seed(random_state)
    np.random.shuffle(bins_index_under)
    # np.random.seed(random_state)
    np.random.shuffle(bins_index_upper)
    hardness_index = concatenate(bins_index_upper, bins_index_under)

    # 拆分
    n_divider_index, rdd_index = divide_and_abandon(hardness_index, group_len)

    # 拆分时多余(低难度)数据均分
    n_divider_index = n_divider_index[::-1]
    n_divider_index = average_allot(n_divider_index, rdd_index)
    n_divider_index = n_divider_index[::-1]
    # n_divider_index = divide_and_resample(hardness_index, group_len)
    return n_divider_index


def divide_uniform(hardness, group_len, random_state):
    bins_index_under, bins_index_upper, bins_index_top = distinguish_regions(hardness, group_len)
    bins_index_down = concatenate(bins_index_under, bins_index_upper)
    np.random.seed(random_state)
    np.random.shuffle(bins_index_down)
    # 拆分
    divide_index_down, abandon_index_down = divide_and_abandon(bins_index_down, group_len)

    n_divider_index = divide_index_down
    n_abandon_index = concatenate(bins_index_top, abandon_index_down)
    n_divider_index = average_allot(n_divider_index, n_abandon_index)
    return n_divider_index


def divide_random(hardness, group_len, random_state):
    hardness_index = np.array(range(len(hardness)))
    np.random.seed(random_state)
    np.random.shuffle(hardness_index)
    # # 拆分
    # n_divider_index, rdd_index = divide_and_abandon(hardness_index, group_len)
    # # # 拆分时不丢弃多余数据
    # n_divider_index = average_allot(n_divider_index, rdd_index)
    n_divider_index = divide_and_resample(hardness_index, group_len, random_state)
    return n_divider_index


# def divide_firm(hardness, group_len, seed):
#     hardness_index = np.array(range(len(hardness)))
#     np.random.seed(seed)
#     np.random.shuffle(hardness_index)
#     # 拆分
#     # n_divider_index, rdd_index = divide_and_abandon(hardness_index, group_len)
#     # # 拆分时不丢弃多余数据
#     # n_divider_index = average_allot(n_divider_index, rdd_index)
#     if len(hardness_index) != 0:
#         rdd_num = len(hardness_index) % group_len
#         group_num = len(hardness_index) // group_len
#         if group_num < 1:
#             n_divide_index = np.array([hardness_index])
#         else:
#             divide_index = hardness_index[:-rdd_num] if rdd_num != 0 else hardness_index
#             n_divide_index = np.array_split(divide_index, group_num)
#             np.random.seed(seed)
#             resample_index = np.concatenate((hardness_index[-rdd_num:], np.random.choice(divide_index, size=group_len-rdd_num, replace=False)), axis=0) if rdd_num != 0 else np.array([])
#             n_divide_index.append(resample_index)
#     else:
#         n_divide_index = np.array([])
#     return n_divide_index

def hash_with_seed(data, seed):
    import hashlib
    # 将随机种子和数据合并在一起
    data = str(seed) + str(data)
    # 计算哈希值并转换为整数
    return int(hashlib.sha256(data.encode()).hexdigest(), 16)


def divide_remainder_hash(X, group_num, random_state=42):
    """ 根据余数进行拆分 """
    X_parity = X.sum(axis=1)
    # 创建一个向量化的哈希函数
    v_hash_with_seed = np.vectorize(hash_with_seed)
    # 创建一个和X_parity相同长度的种子数组
    seeds = np.full(len(X_parity), random_state)
    # 对数组中的每个元素进行哈希
    hashed_data = v_hash_with_seed(X_parity, seeds)
    n_divider_index = []
    for i in range(group_num):
        X_i = np.where(hashed_data % group_num == i)[0]
        if len(X_i) != 0:
            n_divider_index.append(X_i)
    return n_divider_index


def divide_remainder(X, group_num):
    """ 根据余数进行拆分 """
    X_parity = X.sum(axis=1)
    n_divider_index = []
    for i in range(group_num):
        X_i = np.where(X_parity % group_num == i)[0]
        if len(X_i) != 0:
            n_divider_index.append(X_i)
    return n_divider_index


def no_divide(hardness):
    hardness_index = np.array(range(len(hardness)))
    # 不拆分
    np.random.shuffle(hardness_index)
    no_divider_index = np.array([hardness_index])
    return no_divider_index



def divide_repeat(hardness_failed_remained, failed_num, method, itimes, repeat_times):
    random.seed(itimes)
    random_integers = np.random.randint(1, 100, size=repeat_times)
    divider = []
    for i in range(len(random_integers)):
        divider = divider + list(divide_base(hardness_failed_remained, failed_num, method, random_integers[i]))
    return np.array(divider)

def divide_base(hardness_input, group_len, method='random', random_state=42):
    # If hardness values are not distinguishable, perform random sampling
    hardness = deepcopy(hardness_input)
    if len(hardness) > group_len:
        if hardness.max() == hardness.min():
            n_divider_index = divide_random(hardness, group_len, random_state)
        else:
            if method == 'random':
                n_divider_index = divide_random(hardness, group_len, random_state)
            elif method == 'uniform':
                n_divider_index = divide_uniform(hardness, group_len, random_state)
            elif method == 'nonuniform':
                n_divider_index = divide_part_nonuniform(hardness, group_len, random_state)
            elif method == 'gradient':
                n_divider_index = divide_gradient(hardness, group_len, random_state)
            else:
                raise Exception('不在定义好的方法列表中')
    else:
        n_divider_index = np.array([np.arange(len(hardness))])

    return n_divider_index


def cross_integrate(y_passed, y_failed, passed_list, failed_list):
    # 根据model对passed和failed进行拆分
    divider = []
    choosed_index = []
    for i_group in range(len(passed_list)):
        for j_group in range(len(failed_list)):
            if len(passed_list[i_group]) != 0 and len(failed_list[j_group]) != 0:
                two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
                choosed_index.append((i_group, j_group))
                divider.append(two_indexes)
    return choosed_index, divider


def single_integrate(y_passed, y_failed, passed_list, failed_list):
    divider = []
    choosed_index = []
    if len(passed_list) == len(failed_list):
        for j_group in range(len(failed_list)):
            i_group = j_group
            if len(passed_list[i_group]) != 0 and len(failed_list[j_group]) != 0:
                two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
                choosed_index.append((i_group, j_group))
                divider.append(two_indexes)
    else:
        return simple_integrate(y_passed, y_failed, passed_list, failed_list)
    return choosed_index, divider


def simple_integrate(y_passed, y_failed, passed_list, failed_list, max_group_num=800):
    divider = []
    choosed_index = []
    if len(passed_list) > len(failed_list):
        for j_group in range(len(failed_list)):
            i_group = j_group
            # i_group = random.choice(range(len(passed_list)))
            two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
            choosed_index.append((i_group, j_group))
            divider.append(two_indexes)
            # if len(choosed_index) >= max_group_num:
            #     return choosed_index, divider
    else:
        for i_group in range(len(passed_list)):
            j_group = i_group
            # j_group = random.choice(range(len(failed_list)))
            two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
            choosed_index.append((i_group, j_group))
            divider.append(two_indexes)
            # if len(choosed_index) >= max_group_num:
            #     return choosed_index, divider
    return choosed_index, divider


def integrate(y_passed, y_failed, passed_list, failed_list, random_state=42):

    if random_state != 42:
        random.seed(random_state)
        random.shuffle(passed_list)
        random.shuffle(failed_list)

    divider = []
    choosed_index = []
    if len(passed_list) > len(failed_list):
        chosed_num = 1
        temp_num = 0
        for i_group in range(len(passed_list)):
            for j in range(temp_num, temp_num + chosed_num):
                j_group = j % len(failed_list)
                two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
                choosed_index.append((i_group, j_group))
                divider.append(two_indexes)
            temp_num += chosed_num
    else:
        chosed_num = 1
        temp_num = 0
        for j_group in range(len(failed_list)):
            for i in range(temp_num, temp_num + chosed_num):
                i_group = i % len(passed_list)
                two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
                choosed_index.append((i_group, j_group))
                divider.append(two_indexes)
            temp_num += chosed_num
    return choosed_index, divider


def part_integrate(y_passed, y_failed, passed_list, failed_list, total_num, random_state=42):
    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    choosed_index = []
    random.seed(random_state)
    random.shuffle(passed_list)
    random.shuffle(failed_list)
    if len(passed_list) * len(failed_list) <= total_num:
        return cross_integrate(y_passed, y_failed, passed_list, failed_list)
    if len(passed_list) > len(failed_list):
        chosed_num = math.ceil(total_num / len(passed_list))
        temp_num = 0
        for i_group in range(len(passed_list)):
            # chosed_failed_list_index = random.sample(range(len(failed_list)), chosed_num)
            for j in range(temp_num, temp_num + chosed_num):
                j_group = j % len(failed_list)
                two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
                choosed_index.append((i_group, j_group))
                divider.append(two_indexes)
            temp_num += chosed_num
    else:
        chosed_num = math.ceil(total_num / len(failed_list))
        temp_num = 0
        for j_group in range(len(failed_list)):
            # chosed_passed_list_index = random.sample(range(len(passed_list)), chosed_num)
            for i in range(temp_num, temp_num + chosed_num):
                i_group = i % len(passed_list)
                two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
                choosed_index.append((i_group, j_group))
                divider.append(two_indexes)
            temp_num += chosed_num
    return choosed_index, divider


def divider2list(failed_remained_index, failed_divider):
    failed_list = []
    for k_group in range(len(failed_divider)):
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[k_group]])
        failed_list.append(failed_indexes)
    return failed_list


def cluster_divide(X, group_num):
    n_clusters = group_num
    y_pred = KMeans(n_clusters=n_clusters, random_state=42).fit_predict(X)
    # y_pred = GaussianMixture(n_components=n_clusters).fit_predict(X)
    cluster = []
    x_index = np.arange(len(X))
    for c in range(n_clusters):
        cluster.append(x_index[y_pred == c])
    return np.array(cluster)


# region DDU


def DDU(X):
    """ 分为三部分：Density, Diversity, Uniqueness """

    def density(X):
        bool_matrix = (X != 0)
        non_zero_num = bool_matrix.sum()
        return 1 - math.fabs(1 - 2 * non_zero_num / (X.shape[0] * X.shape[1]))

    def diversity(X):
        num = 0
        unq, count = np.unique(X, axis=0, return_counts=True)
        for i in range(len(unq)):
            num += count[i] * (count[i] - 1)
        Gini_simpson = 1 - num / (len(X) * (len(X) - 1)) if len(X) != 1 else 1
        return Gini_simpson

    def new_diversity(X):
        unq, count = np.unique(X, axis=0, return_counts=True)

        return len(unq)

    def uniqueness(X):
        new_X = deepcopy(X)
        new_X = np.transpose(new_X)
        unq, count = np.unique(new_X, axis=0, return_counts=True)
        return len(unq) / len(new_X)

    # density = density(X)
    # diversity = diversity(X)
    # diversity = new_diversity(X)
    uniqueness = uniqueness(X)
    # DDU = density * diversity * uniqueness
    DDU = uniqueness
    return DDU


def list2singlespectra(the_list, X_the):
    the_spectra = []
    for indexes in the_list:
        iX_the = X_the[indexes]
        i_spectra = single_spectra(iX_the)
        the_spectra.append(i_spectra)
    return the_spectra


def single_spectra(iX_the):
    new_X_the = deepcopy(iX_the)
    a_e = new_X_the.sum(axis=0)
    a_n = len(new_X_the) - a_e
    spectra = np.array([a_e, a_n])
    spectra = np.transpose(spectra)
    return spectra


def spectra(X_passed, X_failed):
    new_X_passed = deepcopy(X_passed)
    new_X_failed = deepcopy(X_failed)
    a_ep = new_X_passed.sum(axis=0)
    a_np = len(new_X_passed) - a_ep
    a_ef = new_X_failed.sum(axis=0)
    a_nf = len(new_X_failed) - a_ef
    spectra = np.array([a_ep, a_np, a_ef, a_nf])
    spectra = np.transpose(spectra)
    return spectra


def uniqueness(spectra):
    unq, count = np.unique(spectra, axis=0, return_counts=True)
    return len(unq) / len(spectra)


# endregion

def normalize(a):
    """normalize an array
    :param a: The array to normalize
    """
    return (a - min(a)) / (max(a) - min(a))


def find_divide_num(x, y, polynomial_degree=7):
    # Step 0: preprocess
    x = np.array(x)
    y = np.array(y)
    new_x = np.array(list(dict.fromkeys(x)))
    new_y = np.array([])
    for num_x in new_x:
        new_y = np.append(new_y, np.average(y[np.where(x == num_x)]))
    # new_x = np.insert(new_x, 0, 0)
    # new_y = np.insert(new_y, 0, 0)

    # Step 1: fit a smooth line
    # p = np.poly1d(np.polyfit(new_x, new_y, polynomial_degree))
    # Ds_y = p(new_x)

    # Step 2: normalize values
    # y_normalized = normalize(new_y)

    # Step 3: Calculate the Difference curve
    y_difference = np.diff(new_y) / np.diff(new_x)
    y_difference = np.insert(y_difference, 0, new_y[0])

    # Step 4: find the final index where y_difference is no less than 1/len
    # y_accumulation = np.array([new_y[i] / new_x[i] for i in range(len(new_x))])
    # the_index = np.where(y_difference >= 0.1 * y_difference[0])[0][-1]
    # the_index = np.where(y_accumulation == max(y_accumulation))[0][0]
    the_index = np.where(y_difference >= 0.5 * y_difference[0])[0][-1]
    # model = knee_locator.KneeLocator(x=new_x, y=new_y, curve='concave', S=2, direction='increasing', online=False)
    if the_index == 0 or the_index == len(new_x) - 1:
        divide_num = new_x[-1]
    else:
        divide_num = new_x[the_index + 1]

    # Step 5: plot divide_num
    # plt.figure(figsize=(6, 6))
    # plt.title("Knee Point")
    # plt.plot(new_x, new_y, "b", label="data")
    # plt.vlines(
    #     divide_num, plt.ylim()[0], plt.ylim()[1], linestyles="--", label="divide_num"
    # )
    # plt.legend(loc="best")
    # plt.show()

    return divide_num


def find_divide_num_using_trend(x, y, program_len, polynomial_degree=7):
    # Step 0: preprocess
    x = np.array(x)
    y = np.array(y)
    new_x = np.array(list(dict.fromkeys(x)))
    new_y = np.array([])
    new_divide = []
    for num_x in new_x:
        new_y = np.append(new_y, np.average(y[np.where(x == num_x)] * program_len))
    new_x = np.insert(new_x, 0, 0)
    new_y = np.insert(new_y, 0, 0)

    # Step 1: fit a smooth line
    # p = np.poly1d(np.polyfit(new_x, new_y, polynomial_degree))
    # y_pred = p(new_x)

    # Step 2: normalize values
    # y_normalized = normalize(new_y)

    if is_decreasing_trend(new_x, new_y):

        # fit and find the max
        # y_fit, y_max = predict_max(new_x, new_y)

        # Step 4: Calculate the Difference curve
        y_difference = np.diff(new_y) / np.diff(new_x)
        # y_difference_fit = np.diff(y_fit) / np.diff(new_x)
        # y_difference_pred = np.diff(y_pred) / np.diff(new_x)

        # Step 4: find the final index where y_difference is no less than 1/3
        the_index = np.where(y_difference >= 1.5)[0][-1] if max(y_difference) >= 1.5 else len(new_x) - 1
        # the_index = np.where(y_difference_fit <= 4)[0][0] if min(y_difference_fit) <= 4 else len(new_x) - 1
        # the_index = np.where(new_y >= 40)[0][0] if max(new_y) >= 40 else len(new_x) - 1
        # model = knee_locator.KneeLocator(x=new_x, y=new_y, curve='concave', S=2, direction='increasing', online=False)
        if the_index == len(y_difference) - 1:
            divide_num = new_x[-1]
        else:
            divide_num = new_x[the_index + 2]
    else:
        divide_num = new_x[-1]

    return divide_num


def find_fixed_divide_num(x, y):
    # Step 0: preprocess
    x = np.array(x)
    y = np.array(y)
    new_x = np.array(list(dict.fromkeys(x)))
    new_y = np.array([])
    for num_x in new_x:
        new_y = np.append(new_y, np.average(y[np.where(x == num_x)]))
    new_x = np.insert(new_x, 0, 0)
    new_y = np.insert(new_y, 0, 0)

    # Step 1: fit a smooth line
    # p = np.poly1d(np.polyfit(new_x, new_y, polynomial_degree))
    # y_pred = p(new_x)

    # Step 2: normalize values
    # y_normalized = normalize(new_y)

    # fit and find the max
    # y_fit, y_max = predict_max(new_x, new_y)

    # Step 4: Calculate the Difference curve
    y_difference = np.diff(new_y) / np.diff(new_x)
    # y_difference_fit = np.diff(y_fit) / np.diff(new_x)
    # y_difference_pred = np.diff(y_pred) / np.diff(new_x)

    # Step 4: find the final index where y_difference is no less than 1/3
    for i_x in range(len(new_x[1:])):
        num_x = new_x[i_x + 1]
        if y_difference[i_x] < y_difference[0]:
            return num_x
    return new_x[-1]


def find_divide_num_using_difference(x, y, min_divide_num, max_divide_num):
    # Step 0: preprocess
    x = np.array(x)
    y = np.array(y)
    new_x = np.array(list(dict.fromkeys(x)))
    new_y = np.array([])
    for num_x in new_x:
        new_y = np.append(new_y, np.average(y[np.where(x == num_x)]))
    new_x = np.insert(new_x, 0, 0)
    new_y = np.insert(new_y, 0, 0)

    # Step 1: fit a smooth line
    # p = np.poly1d(np.polyfit(new_x, new_y, polynomial_degree))
    # y_pred = p(new_x)

    # Step 2: normalize values
    # y_normalized = normalize(new_y)

    # fit and find the max
    # y_fit, y_max = predict_max(new_x, new_y)

    # Step 4: Calculate the Difference curve
    y_difference = np.diff(new_y) / np.diff(new_x)
    # y_difference_fit = np.diff(y_fit) / np.diff(new_x)
    # y_difference_pred = np.diff(y_pred) / np.diff(new_x)

    # Step 4: find the final index where y_difference is no less than 1/3
    for i_x in range(len(new_x[1:])):
        num_x = new_x[i_x + 1]
        if min_divide_num <= num_x <= max_divide_num:
            if y_difference[i_x] < y_difference[0]:
                return num_x
    return new_x[-1]


def predict_max(new_x, new_y):
    def exp_func(x, a, b, c):
        return a / (b + x) + c

    try:
        popt, pcov = curve_fit(exp_func, new_x, new_y, p0=[-1, 1, 1], maxfev=5000)
        # Generate predicted values
        y_pred = exp_func(new_x, *popt)
        # Calculate residuals
        residuals = new_y - y_pred
        # Calculate the residual sum of squares
        rss = np.sum(residuals ** 2)
        # Calculate the R-squared value
        y_mean = np.mean(new_y)
        r_squared = 1 - (rss / np.sum((new_y - y_mean) ** 2))
        # Calculate the Root Mean Squared Error
        rmse = np.sqrt(np.mean((y_pred - new_y) ** 2))
        # print("RSS: ", rss)
        # print("R^2: ", r_squared)
        # print("RMSE: ", rmse)
        # Plot the data points and the fit
        plt.scatter(new_x, new_y, label='Data')
        plt.plot(new_x, y_pred, label='Fit')
        plt.legend()
        # plt.show()

        y_max = popt[2]
        return y_pred, y_max
    except:
        y_max = max(new_y)
        return new_y, y_max


def predict_max2(new_x, new_y, polynomial_degree=10):
    # fit a quadratic model
    coef = np.polyfit(new_x, new_y, polynomial_degree)
    equation = np.poly1d(coef)
    y_pred = np.polyval(equation, new_x)
    # Calculate residuals
    residuals = new_y - y_pred
    # Calculate the residual sum of squares
    rss = np.sum(residuals ** 2)
    # Calculate the R-squared value
    y_mean = np.mean(new_y)
    r_squared = 1 - (rss / np.sum((new_y - y_mean) ** 2))
    # Calculate the Root Mean Squared Error
    rmse = np.sqrt(np.mean((y_pred - new_y) ** 2))
    print("RSS: ", rss)
    print("R^2: ", r_squared)
    print("RMSE: ", rmse)
    # Plot the data points and the fit
    plt.scatter(new_x, new_y, label='Data')
    plt.plot(new_x, y_pred, label='Fit')
    plt.legend()
    plt.show()

    # calculate the roots of the derivative of the quadratic
    y_max = equation(1000)
    return np.max(y_max)


def is_decreasing_trend(new_x, y):
    y_difference = np.diff(y) / np.diff(new_x)

    order = 1
    index = [i for i in range(1, len(y_difference) + 1)]
    coeffs = np.polyfit(index, list(y_difference), order)
    slope = coeffs[-2]
    if float(slope) < 0:
        return True
    return False

    # moving_average = np.convolve(y_difference, np.ones(window) / window, mode='same')
    # trend = np.diff(moving_average)
    # check if the slope is negative
    # if np.sum(trend) < 0:
    #     return True
    # return False


def outside_divide_ratio_func(optimal_min_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                              hardness_passed_remained, X_failed, X_passed):
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_num = min(optimal_min_num, len(failed_remained_index))
        failed_group_num = max(1, len(failed_remained_index) // failed_num)
        imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
        passed_group_num = imbalanced_ratio * failed_group_num  # if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
    else:
        passed_num = min(optimal_min_num, len(passed_remained_index))
        passed_group_num = max(1, len(passed_remained_index) // passed_num)
        imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
        failed_group_num = imbalanced_ratio * passed_group_num
        failed_num = max(1, len(failed_remained_index) // failed_group_num)

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    if len(failed_remained_index) <= len(passed_remained_index):
        sum_failed_diagnosability = 0
        for i in range(len(failed_list)):
            failed_index = failed_list[i]
            i_X_failed = X_failed[failed_index]
            i_X_spectra = single_spectra(i_X_failed)
            sum_failed_diagnosability += uniqueness(i_X_spectra)
        average_num = len(failed_remained_index) / len(failed_list)
        average_diagnosability = sum_failed_diagnosability / len(failed_list)
    else:
        sum_passed_diagnosability = 0
        for i in range(len(passed_list)):
            passed_index = passed_list[i]
            i_X_passed = X_passed[passed_index]
            i_X_spectra = single_spectra(i_X_passed)
            sum_passed_diagnosability += uniqueness(i_X_spectra)
        average_num = len(passed_remained_index) / len(passed_list)
        average_diagnosability = sum_passed_diagnosability / len(passed_list)

    return average_num, average_diagnosability


def enlarge_failed_divider(failed_divider, total_num, first_ratio=0.5, random_state=42):
    first_failed_divider, last_failed_divider = failed_divider[
                                                :math.floor(len(failed_divider) * first_ratio)], failed_divider[
                                                                                                 math.floor(
                                                                                                     len(failed_divider) * first_ratio):]
    # 计算每个数组应重复的整数倍数
    int_repeats = int(np.floor((total_num - len(last_failed_divider)) / len(first_failed_divider))) if len(
        first_failed_divider) != 0 else 0
    # 计算应随机抽取的数目
    last_repeat = int(total_num - len(last_failed_divider) - len(first_failed_divider) * int_repeats)
    chosed_divider = []
    random.seed(random_state)
    for _ in range(last_repeat):
        if len(first_failed_divider) != 0:
            chosed_divider.append(random.choice(list(first_failed_divider)))
    # 扩充数组A
    failed_divider = list(first_failed_divider) * int_repeats + chosed_divider + list(last_failed_divider)
    return np.array(failed_divider)


def repeat_divider(divider, total_num, random_state=42):
    # 计算每个数组应重复的整数倍数
    int_repeats = int(np.floor(total_num / len(divider))) if len(divider) != 0 else 0
    # 计算应随机抽取的数目
    last_repeat = int(total_num - len(divider) * int_repeats)
    chosed_divider = []
    random.seed(random_state)
    for _ in range(last_repeat):
        if len(divider) != 0:
            chosed_divider.append(random.choice(list(divider)))
    # 扩充数组A
    failed_divider = list(divider) * int_repeats + chosed_divider
    return np.array(failed_divider)


def outside_divide_func2(optimal_min_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                         hardness_passed_remained, y_failed, y_passed, itimes):
    # confident_ratio = 0.1
    optimal_min_num = int(optimal_min_num)
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_num = min(optimal_min_num, len(failed_remained_index))
        failed_group_num = max(1, len(failed_remained_index) // failed_num)
        # failed_group_num = 10
        # imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
        imbalanced_ratio = 2
        passed_group_num = math.ceil(imbalanced_ratio * failed_group_num) if failed_group_num != 1 else 1
        # passed_group_num = failed_group_num + 1 if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        if len(failed_remained_index) // failed_num > 1:
            # index_array = np.arange(len(hardness_failed_remained))  # 创建索引数组
            # sorted_indices = np.argsort(hardness_failed_remained)  # 排序后的索引数组
            # length = len(hardness_failed_remained)  # 数组长度
            # split_index = math.ceil(confident_ratio * length)  # 前%的位置索引
            # first_indices = index_array[sorted_indices[:split_index]]  # 获取前%的索引数组
            # np.random.seed(itimes)
            # np.random.shuffle(first_indices)
            failed_divider = divide_repeat(hardness_failed_remained, failed_num, 'random', itimes, 20)
            passed_divider = divide_repeat(hardness_passed_remained, passed_num, 'random', itimes, 20)
            # failed_divider_original = divide_base(hardness_failed_remained, failed_num, 'random', itimes)
            # passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)
            # other_failed_divider = divide_and_resample(first_indices, failed_num, itimes)
            # failed_divider = np.array(list(
            #     repeat_divider(other_failed_divider, len(passed_divider) - len(failed_divider_original),
            #                    itimes)) + list(failed_divider_original))
        else:
            failed_divider = no_divide(hardness_failed_remained)
            passed_divider = no_divide(hardness_passed_remained)
    else:
        failed_array = np.arange(len(failed_remained_index))
        passed_array = np.arange(len(passed_remained_index))
        passed_num = min(optimal_min_num, len(passed_array))
        passed_group_num = max(1, len(passed_array) // passed_num)
        failed_group_num = passed_group_num
        failed_num = max(1, len(failed_array) // failed_group_num)
        failed_divider = divide_repeat(hardness_failed_remained, failed_num, 'random', itimes, 20)
        passed_divider = divide_repeat(hardness_passed_remained, passed_num, 'random', itimes, 20)

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    # choosed_index, divider = cross_integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800, itimes)
    # choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list, itimes)
    return divider

def outside_divide_func_group(optimal_group_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                         hardness_passed_remained, y_failed, y_passed, itimes):
    # confident_ratio = 0.1
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_group_num = max(1, optimal_group_num)
        failed_num = max(1, len(failed_remained_index) // failed_group_num)
        # failed_group_num = 10
        # imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
        imbalanced_ratio = 3
        passed_group_num = math.ceil(imbalanced_ratio * failed_group_num) if failed_group_num != 1 else 1
        # passed_group_num = failed_group_num + 1 if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        if len(failed_remained_index) // failed_num > 1:
            # index_array = np.arange(len(hardness_failed_remained))  # 创建索引数组
            # sorted_indices = np.argsort(hardness_failed_remained)  # 排序后的索引数组
            # length = len(hardness_failed_remained)  # 数组长度
            # split_index = math.ceil(confident_ratio * length)  # 前%的位置索引
            # first_indices = index_array[sorted_indices[:split_index]]  # 获取前%的索引数组
            # np.random.seed(itimes)
            # np.random.shuffle(first_indices)
            failed_divider = divide_repeat(hardness_failed_remained, failed_num, 'random', itimes, 20)
            passed_divider = divide_repeat(hardness_passed_remained, passed_num, 'random', itimes, 20)
            # failed_divider_original = divide_base(hardness_failed_remained, failed_num, 'random', itimes)
            # passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)
            # other_failed_divider = divide_and_resample(first_indices, failed_num, itimes)
            # failed_divider = np.array(list(
            #     repeat_divider(other_failed_divider, len(passed_divider) - len(failed_divider_original),
            #                    itimes)) + list(failed_divider_original))
        else:
            failed_divider = no_divide(hardness_failed_remained)
            passed_divider = no_divide(hardness_passed_remained)
    else:
        failed_array = np.arange(len(failed_remained_index))
        passed_array = np.arange(len(passed_remained_index))
        passed_group_num = optimal_group_num
        passed_num = max(1, len(passed_array) // passed_group_num)
        failed_group_num = passed_group_num
        failed_num = max(1, len(failed_array) // failed_group_num)
        failed_divider = divide_repeat(hardness_failed_remained, failed_num, 'random', itimes, 20)
        passed_divider = divide_repeat(hardness_passed_remained, passed_num, 'random', itimes, 20)

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    # choosed_index, divider = cross_integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800, itimes)
    # choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list, itimes)
    return divider

def outside_divide_func_robust(optimal_group_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                               hardness_passed_remained, X_failed_remained, X_passed_remained, y_failed, y_passed,
                               itimes):
    # confident_ratio = 0.1
    optimal_group_num = int(optimal_group_num)
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_group_num = optimal_group_num
        # failed_group_num = min(5, max(1, math.floor(len(failed_remained_index) / failed_num)))
        # failed_group_num = 5
        # imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
        imbalanced_ratio = 1
        passed_group_num = math.ceil(imbalanced_ratio * failed_group_num) if failed_group_num != 1 else 1
        if failed_group_num > 1:
            # index_array = np.arange(len(hardness_failed_remained))  # 创建索引数组
            # sorted_indices = np.argsort(hardness_failed_remained)  # 排序后的索引数组
            # length = len(hardness_failed_remained)  # 数组长度
            # split_index = math.ceil(confident_ratio * length)  # 前%的位置索引
            # first_indices = index_array[sorted_indices[:split_index]] # 获取前%的索引数组
            # np.random.seed(itimes)
            # np.random.shuffle(first_indices)
            failed_divider = divide_remainder_hash(X_failed_remained, failed_group_num, itimes)
            passed_divider = divide_remainder_hash(X_passed_remained, passed_group_num, itimes)
            # failed_divider_original = divide_base(hardness_failed_remained, failed_num, 'random', itimes)
            # passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)
            # other_failed_divider = divide_and_resample(first_indices, failed_num, itimes)
            # failed_divider = np.array(list(repeat_divider(other_failed_divider, len(passed_divider)-len(failed_divider_original), itimes)) + list(failed_divider_original))
        else:
            failed_divider = no_divide(hardness_failed_remained)
            passed_divider = no_divide(hardness_passed_remained)
    else:
        # failed_array = np.arange(len(failed_remained_index))
        # passed_array = np.arange(len(passed_remained_index))
        passed_group_num = optimal_group_num
        failed_group_num = passed_group_num
        # failed_num = max(1, len(failed_array) // failed_group_num)
        # failed_divider = divide_base(hardness_failed_remained, failed_num, 'random', itimes)
        # passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)
        failed_divider = divide_remainder_hash(X_failed_remained, failed_group_num, itimes)
        passed_divider = divide_remainder_hash(X_passed_remained, passed_group_num, itimes)

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    # choosed_index, divider = cross_integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list, itimes)
    return divider


def outside_divide_func(optimal_min_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                        hardness_passed_remained, y_failed, y_passed, itimes):
    confident_ratio = 0.5
    optimal_min_num = int(optimal_min_num)
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_num = min(optimal_min_num, len(failed_remained_index))
        failed_group_num = max(1, len(failed_remained_index) // failed_num)
        # imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
        imbalanced_ratio = 1
        passed_group_num = imbalanced_ratio * failed_group_num if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        # failed_divider = divide_base(hardness_failed_remained, failed_num, 'random', itimes)
        # passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)
        if len(failed_remained_index) // failed_num > 1:
            failed_divider_original = divide_base(hardness_failed_remained, failed_num, 'nonuniform', itimes)
            passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)
            failed_divider = enlarge_failed_divider(failed_divider_original, len(passed_divider), confident_ratio,
                                                    itimes)
        else:
            failed_divider = no_divide(hardness_failed_remained)
            passed_divider = no_divide(hardness_passed_remained)
    else:
        failed_array = np.arange(len(failed_remained_index))
        passed_array = np.arange(len(passed_remained_index))
        passed_num = min(optimal_min_num, len(passed_array))
        passed_group_num = max(1, len(passed_array) // passed_num)
        failed_group_num = passed_group_num
        failed_num = max(1, len(failed_array) // failed_group_num)
        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random', itimes)
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random', itimes)

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    choosed_index, divider = cross_integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    # choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    return divider


def outside_divide_func_backup(optimal_min_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                               hardness_passed_remained, y_failed, y_passed, itimes):
    confident_ratio = 0.5
    optimal_min_num = int(optimal_min_num)
    if len(failed_remained_index) <= len(passed_remained_index):
        first_failed, last_failed = distinguish_two_regions(hardness_failed_remained, confident_ratio)
        # first_failed_index, last_failed_index = [failed_remained_index[index] for index in first_failed], [
        #     failed_remained_index[index] for index in last_failed]
        aug_ratio = (len(passed_remained_index) / len(
            failed_remained_index) + confident_ratio - 1) / confident_ratio
        aug_num = int(aug_ratio)
        aug_numerical = len(passed_remained_index) - len(first_failed) * aug_num - len(last_failed)
        random.seed(itimes)
        failed_array = np.array(list(first_failed) * aug_num + [random.choice(first_failed) for _ in
                                                                range(aug_numerical)] + list(last_failed))
        passed_array = np.arange(len(passed_remained_index))

        failed_num = min(optimal_min_num, len(failed_array))
        failed_group_num = max(1, len(failed_array) // failed_num)
        # magnification_factor = 1     # 放大系数由清理效果决定，若保守估计清理效果有50%，则放大系数可定为2
        passed_group_num = failed_group_num
        passed_num = max(failed_num, len(passed_array) // passed_group_num)
        average_num = len(failed_array) / failed_group_num
    else:
        failed_array = np.arange(len(failed_remained_index))
        passed_array = np.arange(len(passed_remained_index))
        passed_num = min(optimal_min_num, len(passed_array))
        passed_group_num = max(1, len(passed_array) // passed_num)
        failed_group_num = passed_group_num
        failed_num = max(1, len(failed_array) // failed_group_num)
        average_num = len(passed_array) / passed_group_num

    hardness_passed_remained = hardness_passed_remained[passed_array]
    hardness_failed_remained = hardness_failed_remained[failed_array]

    temp_failed_divider = divide_base(hardness_failed_remained, failed_num, 'nonuniform', itimes)
    temp_passed_divider = divide_base(hardness_passed_remained, passed_num, 'nonuniform', itimes)

    failed_divider = np.array([failed_array[divider] for divider in temp_failed_divider])
    passed_divider = np.array([passed_array[divider] for divider in temp_passed_divider])

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    # choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    return average_num, divider


def outside_divide_func_origin(optimal_min_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                               hardness_passed_remained, y_failed, y_passed, itimes):
    optimal_min_num = int(optimal_min_num)
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_num = min(optimal_min_num, len(failed_remained_index))
        failed_group_num = max(1, len(failed_remained_index) // failed_num)
        magnification_factor = 1  # 放大系数由清理效果决定，若保守估计清理效果有50%，则放大系数可定为2
        passed_group_num = magnification_factor * failed_group_num if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
    else:
        passed_num = min(optimal_min_num, len(passed_remained_index))
        passed_group_num = max(1, len(passed_remained_index) // passed_num)
        failed_group_num = passed_group_num
        failed_num = max(1, len(failed_remained_index) // failed_group_num)

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'nonuniform', itimes)
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'nonuniform', itimes)

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    # passed_list = aug_list(passed_list, len(passed_list))
    # failed_list = aug_list(failed_list, len(failed_list))

    # choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    choosed_index, divider = cross_integrate(y_passed, y_failed, passed_list, failed_list)
    # choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    return divider


def group_cluster_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 0.9, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 0.9, least_num=1)
    X_filed_remaied = np.array([X_failed[index] for index in failed_remained_index])

    # HDBSCAN对失例聚类
    cluster = hdbscan.HDBSCAN(min_cluster_size=5, allow_single_cluster=True)
    cluster.fit(X_filed_remaied)
    # 将未成功聚类的离散点也当作其中一个集合
    if cluster.labels_.max() == -1:
        cluster.labels_ = cluster.labels_ + 1
    group_num = cluster.labels_.max() + 1
    failed_num_ratio = np.array([len(cluster.labels_[cluster.labels_ == i]) for i in range(group_num)]) / len(
        cluster.labels_[cluster.labels_ >= 0])
    passed_divider = divide_based_on_ratio(passed_remained_index, failed_num_ratio)

    # # 根据model对passed进行等比例拆分，对failed进行欠采样
    # divider = []
    # for i_group in range(group_num):
    #     failed_indexes = np.array(failed_remained_index[cluster.labels_ == i_group])
    #     passed_indexes = passed_divider[i_group]
    #     two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
    #     divider.append(two_indexes)

    # 根据model对passed进行等比例拆分，对failed进行欠采样
    divider = []
    max_failed = 0
    two_indexes = {str(y_passed[0]): passed_remained_index, str(y_failed[0]): failed_remained_index}
    for i_group in range(group_num):
        failed_indexes = np.array(failed_remained_index[cluster.labels_ == i_group])
        passed_indexes = passed_divider[i_group]
        if len(failed_indexes) > max_failed:
            max_failed = len(failed_indexes)
            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
    divider.append(two_indexes)
    return divider


def group_cluster2_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 0.9, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 0.9, least_num=1)
    group_num = min(5, max(1, math.floor(len(failed_remained_index) / 5)))
    X_filed_remaied = np.array([X_failed[index] for index in failed_remained_index])

    # Kmeans对失例聚类
    cluster = cluster_divide(X_filed_remaied, group_num)

    failed_num_ratio = np.array([len(cluster[i]) for i in range(group_num)]) / len(X_filed_remaied)
    passed_divider = divide_based_on_ratio(passed_remained_index, failed_num_ratio)

    # 对passed进行等比例拆分
    divider = []
    max_failed = 0
    two_indexes = {str(y_passed[0]): passed_remained_index, str(y_failed[0]): failed_remained_index}
    for i_group in range(group_num):
        failed_indexes = np.array([failed_remained_index[index] for index in cluster[i_group]])
        passed_indexes = passed_divider[i_group]
        if len(failed_indexes) > max_failed:
            max_failed = len(failed_indexes)
            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
    divider.append(two_indexes)

    # for i_group in range(group_num):
    #     failed_indexes = np.array([failed_remained_index[index] for index in cluster[i_group]])
    #     passed_indexes = passed_divider[i_group]
    #     two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
    #     divider.append(two_indexes)
    return divider


def group_enlarge_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    # 将失例克隆到成例数量
    failed_cloned_index = cloned(failed_remained_index, len(passed_remained_index))
    # 计算克隆后的每个分类难度
    hardness_failed_cloned = np.array([hardness_failed[index] for index in failed_cloned_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])
    # 计算组数和每组的拆分数量
    group_num = min(5, max(1, math.floor(len(failed_remained_index) / 5)))
    failed_num = math.floor(len(failed_cloned_index) / group_num)
    passed_num = math.floor(len(passed_remained_index) / group_num)
    # 开始拆分
    failed_divider = divide_base(hardness_failed_cloned, failed_num, 'random')
    # passed_divider = no_divide(hardness_passed)
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    for i_group in range(group_num):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
        failed_indexes = np.array([failed_cloned_index[index] for index in failed_divider[i_group]])
        # failed_indexes = random_under_sampling(X_failed, group_len)

        two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
        divider.append(two_indexes)
    return divider


def group_divider_enlarge(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    # group_num = min(5, max(1, math.floor(len(failed_remained_index) / 5)))
    group_num = 1
    failed_num = math.floor(len(failed_remained_index) / group_num)
    passed_num = math.floor(len(passed_remained_index) / group_num)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    passed_list = aug_list(passed_list, 10)
    failed_list = aug_list(failed_list, 10)

    # choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)

    return divider


def group_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    group_num = min(5, max(1, math.floor(len(failed_remained_index) / 5)))
    failed_num = math.floor(len(failed_remained_index) / group_num)
    passed_num = math.floor(len(passed_remained_index) / group_num)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    # passed_divider = no_divide(hardness_passed)
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    for i_group in range(group_num):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])
        # failed_indexes = random_under_sampling(X_failed, group_len)

        two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
        divider.append(two_indexes)
    return divider


def group_cross_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    group_num = min(5, max(1, math.floor(min(len(passed_remained_index), len(failed_remained_index)) / 5)))
    failed_num = math.floor(len(failed_remained_index) / group_num)
    passed_num = math.floor(len(passed_remained_index) / group_num)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    # passed_divider = no_divide(hardness_passed)
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    passed_list = []
    failed_list = []
    for i_group in range(group_num):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])
        # failed_indexes = random_under_sampling(X_failed, group_len)
        passed_list.append(passed_indexes)
        failed_list.append(failed_indexes)
    for i_group in range(len(passed_list)):
        for j_group in range(len(failed_list)):
            two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
            divider.append(two_indexes)
    return divider


def group_max_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    X_failed_remained = np.array([X_failed[index] for index in failed_remained_index])
    X_passed_remained = np.array([X_passed[index] for index in passed_remained_index])
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])
    # ratio = len(failed_remained_index) / (len(failed_remained_index) + len(passed_remained_index))
    # optimal_minority_num = max(3, round(2 * math.sqrt(1 / ratio)))
    # optimal_minority_num = max(3, X.shape[1] / 500)
    # optimal_minority_num = 3
    # optimal_minority_num = 1
    # optimal_majority_num = math.ceil(optimal_minority_num * max(len(failed_remained_index), len(passed_remained_index)) / min(len(failed_remained_index), len(passed_remained_index)))
    # minority_index_len = min(len(failed_remained_index), len(passed_remained_index))
    # group_num = 2 if minority_index_len > 1 else 1
    # group_num = max(1, math.floor(minority_index_len / optimal_minority_num))
    # failed_num = math.floor(len(failed_remained_index) / group_num)
    # passed_num = math.floor(len(passed_remained_index) / group_num)
    # optimal_minority_num = 3
    # if len(failed_remained_index) <= len(passed_remained_index):
    #     optimal_failed_num = optimal_minority_num
    #     failed_divider = divide_base(hardness_failed_remained, optimal_failed_num, 'order')
    #     optimal_passed_num = math.floor(optimal_failed_num * len(passed_remained_index) / len(failed_remained_index))
    if len(failed_remained_index) <= len(passed_remained_index):
        # optimal_failed_num = max(3, round(X.shape[1] / 500))
        optimal_failed_num = 8
        failed_num = min(optimal_failed_num, len(failed_remained_index))
        failed_group_num = max(1, len(failed_remained_index) // failed_num)
        passed_group_num = 2 * failed_group_num  # if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
    else:
        # optimal_passed_num = max(3, round(X.shape[1] / 500))
        optimal_passed_num = 8
        passed_num = min(optimal_passed_num, len(passed_remained_index))
        passed_group_num = max(1, len(passed_remained_index) // passed_num)
        failed_group_num = max(1, passed_group_num // 2)
        failed_num = max(1, len(failed_remained_index) // failed_group_num)

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    # passed_list = aug_list(passed_list, len(passed_list))
    # failed_list = aug_list(failed_list, len(passed_list))

    choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
    # choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)
    return divider


def group_max_parity_divider(hardness, X, y):
    # 初始化
    divider = []
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    X_failed_remained = np.array([X_failed[index] for index in failed_remained_index])
    X_passed_remained = np.array([X_passed[index] for index in passed_remained_index])
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])
    # ratio = len(failed_remained_index) / (len(failed_remained_index) + len(passed_remained_index))
    # optimal_minority_num = max(3, round(2 * math.sqrt(1 / ratio)))
    # optimal_minority_num = max(3, X.shape[1] / 500)
    # optimal_minority_num = 3
    # optimal_minority_num = 1
    # optimal_majority_num = math.ceil(optimal_minority_num * max(len(failed_remained_index), len(passed_remained_index)) / min(len(failed_remained_index), len(passed_remained_index)))
    # minority_index_len = min(len(failed_remained_index), len(passed_remained_index))
    # group_num = 2 if minority_index_len > 1 else 1
    # group_num = max(1, math.floor(minority_index_len / optimal_minority_num))
    # failed_num = math.floor(len(failed_remained_index) / group_num)
    # passed_num = math.floor(len(passed_remained_index) / group_num)
    # optimal_minority_num = 3
    # if len(failed_remained_index) <= len(passed_remained_index):
    #     optimal_failed_num = optimal_minority_num
    #     failed_divider = divide_base(hardness_failed_remained, optimal_failed_num, 'order')
    #     optimal_passed_num = math.floor(optimal_failed_num * len(passed_remained_index) / len(failed_remained_index))
    if min(len(failed_remained_index), len(passed_remained_index)) <= 5:
        two_indexes = {str(y_passed[0]): passed_remained_index, str(y_failed[0]): failed_remained_index}
        divider.append(two_indexes)
    else:
        group_num = 2

        failed_divider = divide_remainder(X_failed_remained, 2)
        passed_divider = divide_remainder(X_passed_remained, 2)

        for i_group in range(group_num):
            passed_indexes = np.array([])
            pass_exid = 0
            while len(passed_indexes) == 0:
                passed_indexes = np.array(
                    [passed_remained_index[index] for index in passed_divider[i_group + pass_exid]])
                pass_exid += 1

            failed_indexes = np.array([])
            fail_exid = 0
            while len(failed_indexes) == 0:
                failed_indexes = np.array(
                    [failed_remained_index[index] for index in failed_divider[i_group + fail_exid]])
                fail_exid += 1

            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
            divider.append(two_indexes)
    return divider


def group_max_uniqueness_divider(hardness, X, y):
    def divide_func(optimal_group_num):
        group_num = max(1, math.floor(min(len(passed_remained_index), len(failed_remained_index)) / optimal_group_num))
        failed_num = math.floor(len(failed_remained_index) / group_num)
        passed_num = math.floor(len(passed_remained_index) / group_num)

        hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
        hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        # 根据model对passed进行拆分，对failed进行欠采样
        divider = []

        sum_diagnosability = 0
        for i_group in range(group_num):
            passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
            failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])
            iX_passed_remained = X_passed[passed_remained_index]
            iX_failed_remained = X_failed[failed_remained_index]
            i_spectra = spectra(iX_passed_remained, iX_failed_remained)
            sum_diagnosability += uniqueness(i_spectra)
            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
            divider.append(two_indexes)
        average_diagnosability = sum_diagnosability / min(len(failed_remained_index), len(passed_remained_index))
        diagnosability_ratio = average_diagnosability / the_diagnosability
        return diagnosability_ratio, divider

    def binary_search(lowerbound, upperbound, ratio):
        if lowerbound >= upperbound:
            the_ratio, divider = divide_func(lowerbound)
            return lowerbound, divider
        mid_num = int((upperbound + lowerbound) / 2)
        the_ratio, divider = divide_func(mid_num)
        if mid_num == lowerbound:
            if the_ratio <= ratio:
                return mid_num, divider
            else:
                the_ratio, divider = divide_func(upperbound)
                return upperbound, divider
        elif the_ratio == ratio:
            return mid_num, divider
        elif the_ratio < ratio:
            return binary_search(lowerbound, mid_num, ratio)
        else:
            return binary_search(mid_num, upperbound, ratio)

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1.1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    X_passed_remained = X_passed[passed_remained_index]
    X_failed_remained = X_failed[failed_remained_index]
    the_spectra = spectra(X_passed_remained, X_failed_remained)
    the_diagnosability = uniqueness(the_spectra)
    max_group_num = math.ceil((min(len(passed_remained_index), len(failed_remained_index)) + 1) / 2)
    final_group_num, divider = binary_search(3, max_group_num, 0.075)
    # the_ratio, divider = divide_func(12)
    return divider


def group_failed_ddu_divider(hardness, X, y):
    def slope_func(optimal_min_num):
        ratio_n1 = ratio_func(optimal_min_num - 1)
        ratio_n2 = ratio_func(optimal_min_num)
        slope = (ratio_n2 - ratio_n1)
        return slope

    def other_ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            new_X_remained = np.vstack((X_failed_remained, X_passed_remained))
        else:
            new_X_remained = np.vstack((X_passed_remained, X_failed_remained))

        i_X = new_X_remained[:optimal_min_num]
        sum_diagnosability = DDU(i_X)

        diagnosability_ratio = sum_diagnosability / the_diagnosability
        return diagnosability_ratio

    def divide_ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            passed_group_num = 2 * failed_group_num
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        else:

            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = max(1, passed_group_num // 2)
            failed_num = max(1, len(failed_remained_index) // failed_group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list, passed_list = divider2list(failed_remained_index, passed_remained_index, failed_divider,
                                                passed_divider)

        sum_failed_diagnosability = 0
        for i in range(len(failed_list)):
            failed_index = failed_list[i]
            i_X_failed = X_failed[failed_index]
            sum_failed_diagnosability += DDU(i_X_failed)

        sum_passed_diagnosability = 0
        for i in range(len(passed_list)):
            passed_index = passed_list[i]
            i_X_passed = X_passed[passed_index]
            sum_passed_diagnosability += DDU(i_X_passed)

        divider = integrate(y_passed, y_failed, passed_list, failed_list)
        average_diagnosability = min(sum_failed_diagnosability / len(failed_list),
                                     sum_passed_diagnosability / len(passed_list))
        diagnosability_ratio = average_diagnosability / the_diagnosability
        return diagnosability_ratio, divider

    def ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            passed_group_num = 2 * failed_group_num
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = max(1, passed_group_num // 2)
            failed_num = max(1, len(failed_remained_index) // failed_group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list, passed_list = divider2list(failed_remained_index, passed_remained_index, failed_divider,
                                                passed_divider)

        sum_failed_diagnosability = 0
        for i in range(len(failed_list)):
            failed_index = failed_list[i]
            i_X_failed = X_failed[failed_index]
            sum_failed_diagnosability += DDU(i_X_failed)

        sum_passed_diagnosability = 0
        for i in range(len(passed_list)):
            passed_index = passed_list[i]
            i_X_passed = X_passed[passed_index]
            sum_passed_diagnosability += DDU(i_X_passed)

        average_diagnosability = min(sum_failed_diagnosability / len(failed_list),
                                     sum_passed_diagnosability / len(passed_list))
        diagnosability_ratio = average_diagnosability / the_diagnosability
        return diagnosability_ratio

    def divide_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            passed_group_num = 2 * failed_group_num
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = max(1, passed_group_num // 2)
            failed_num = max(1, len(failed_remained_index) // failed_group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list, passed_list = divider2list(failed_remained_index, passed_remained_index, failed_divider,
                                                passed_divider)
        divider = integrate(y_passed, y_failed, passed_list, failed_list)
        return divider

    def binary_search(lowerbound, upperbound, ratio):
        if lowerbound >= upperbound:
            # divider = divide_func(lowerbound)
            the_ratio, divider = divide_ratio_func(lowerbound)
            return lowerbound, divider
        mid_num = int((upperbound + lowerbound) / 2)
        # the_ratio = ratio_func(mid_num)
        # # the_ratio = slope_func(mid_num)
        # divider = divide_func(mid_num)
        the_ratio, divider = divide_ratio_func(mid_num)
        if mid_num == lowerbound:
            if the_ratio >= ratio:
                return mid_num, divider
            else:
                # divider = divide_func(upperbound)
                the_ratio, divider = divide_ratio_func(upperbound)
                return upperbound, divider
        elif the_ratio == ratio:
            return mid_num, divider
        elif the_ratio > ratio:
            return binary_search(lowerbound, mid_num, ratio)
        else:
            return binary_search(mid_num, upperbound, ratio)

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    X_failed_remained = X_failed[failed_remained_index]
    X_passed_remained = X_passed[passed_remained_index]
    random.shuffle(X_failed_remained)
    random.shuffle(X_passed_remained)

    X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    X_remained = X[X_remained_index]
    the_diagnosability = DDU(X_remained)
    # target_ddu = 30 / len(X[0])
    min_minority_num = min(len(passed_remained_index), len(failed_remained_index))
    # max_minority_num = min(2 * min_minority_num, len(passed_remained_index) + len(failed_remained_index))
    the_minority_num, divider = binary_search(2, min_minority_num, 0.2)
    # the_minority_num, divider = binary_search(2, min_minority_num, target_ddu)

    # region knee detecting
    # curve_x = []
    # curve_y = []
    # for the_num in range(2, min_minority_num):
    #     curve_x.append(the_num)
    #     curve_y.append(ratio_func(the_num))
    # model = KneeLocator(x=curve_x, y=curve_y, curve='concave', direction='increasing', online=False)
    # if model.knee != curve_x[0] and model.knee is not None:
    #     the_minority_num = min(min_minority_num, model.knee)
    # else:
    #     the_minority_num = min_minority_num
    # endregion

    # the_ratio, divider = divide_ratio_func(14)
    return divider


def group_max_uniqueness_difference_divider(hardness, X, y):
    def min_divide_ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
            passed_group_num = imbalanced_ratio * failed_group_num  # if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
            failed_group_num = imbalanced_ratio * passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        failed_spectra = list2singlespectra(failed_list, X_failed)
        passed_spectra = list2singlespectra(passed_list, X_passed)

        choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)

        sum_diagnosability = 0
        for k_group in range(len(divider)):
            # two_indexes = divider[i_group]
            # passed_indexes = np.array(two_indexes[str(y_passed[0])])
            # failed_indexes = np.array(two_indexes[str(y_failed[0])])
            # iX_passed = X_passed[passed_indexes]
            # iX_failed = X_failed[failed_indexes]
            # i_spectra = spectra(iX_passed, iX_failed)
            i_passed_spectra = passed_spectra[choosed_index[k_group][0]]
            i_failed_spectra = failed_spectra[choosed_index[k_group][1]]
            i_spectra = np.concatenate((i_passed_spectra, i_failed_spectra), axis=1)
            sum_diagnosability += uniqueness(i_spectra)
        average_diagnosability = sum_diagnosability / len(divider)
        return average_num, average_diagnosability

    def divide_ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            # imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
            Magnification_factor = 2  # 放大系数由清理效果决定，若保守估计清理效果有50%，则放大系数可定为2
            passed_group_num = Magnification_factor * failed_group_num  # if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            # imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
            failed_group_num = passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        failed_spectra = list2singlespectra(failed_list, X_failed)
        passed_spectra = list2singlespectra(passed_list, X_passed)

        # passed_list = aug_list(passed_list, len(failed_list))
        # failed_list = aug_list(failed_list, len(failed_list))

        # choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)
        choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 200)

        sum_diagnosability = 0
        for k_group in range(len(divider)):
            # two_indexes = divider[i_group]
            # passed_indexes = np.array(two_indexes[str(y_passed[0])])
            # failed_indexes = np.array(two_indexes[str(y_failed[0])])
            # iX_passed = X_passed[passed_indexes]
            # iX_failed = X_failed[failed_indexes]
            # i_spectra = spectra(iX_passed, iX_failed)
            i_passed_spectra = passed_spectra[choosed_index[k_group][0]]
            i_failed_spectra = failed_spectra[choosed_index[k_group][1]]
            i_spectra = np.concatenate((i_passed_spectra, i_failed_spectra), axis=1)
            sum_diagnosability += uniqueness(i_spectra)
        average_diagnosability = sum_diagnosability / len(divider)
        return average_num, average_diagnosability, divider

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    program_len = len(X[0])
    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    # X_failed_remained = X_failed[failed_remained_index]
    # X_passed_remained = X_passed[passed_remained_index]

    # X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    # X_remained = X[X_remained_index]
    # the_diagnosability = DDU(X_remained)
    min_minority_num = math.ceil((min(len(passed_remained_index), len(failed_remained_index)) + 1) / 2)
    bins_num = 10
    bin_num_list = bin_func(min_minority_num, bins_num)

    curve_x = []
    curve_y = []
    for the_num in bin_num_list:
        true_num, ratio = min_divide_ratio_func(the_num)
        curve_x.append(true_num)
        curve_y.append(ratio)
    the_minority_num = find_divide_num_using_trend(curve_x, curve_y, program_len)
    true_num, ratio, divider = divide_ratio_func(math.floor(the_minority_num))
    print('拆分个数:{}'.format(true_num))
    return divider


def group_fixed_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    X_failed_remained = X_failed[failed_remained_index]
    X_passed_remained = X_passed[passed_remained_index]
    # 设置合理的拆分数量
    optimal_divide_num = math.ceil(math.log10(len(X[0])) / math.log10(2))
    optimal_group_num = 10
    group_num = min(optimal_group_num, max(1, math.floor(
        min(len(passed_remained_index), len(failed_remained_index)) / optimal_divide_num)))
    min_num = min(len(passed_remained_index), len(failed_remained_index)) / group_num

    failed_divider = divide_remainder(X_failed_remained, group_num)
    passed_divider = divide_remainder(X_passed_remained, group_num)

    failed_list = divider2list(failed_remained_index, failed_divider)
    passed_list = divider2list(passed_remained_index, passed_divider)

    # passed_list = aug_list(passed_list, len(failed_list))
    # failed_list = aug_list(failed_list, len(failed_list))

    # choosed_index, divider = single_integrate(y_passed, y_failed, passed_list, failed_list)
    choosed_index, divider = cross_integrate(y_passed, y_failed, passed_list, failed_list)
    print('拆分个数:{}'.format(min_num))
    return divider


def group_fixed_uniqueness_divider(hardness, X, y):
    def min_divide_ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
            passed_group_num = imbalanced_ratio * failed_group_num  # if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
            failed_group_num = imbalanced_ratio * passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        failed_spectra = list2singlespectra(failed_list, X_failed)
        passed_spectra = list2singlespectra(passed_list, X_passed)

        choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 100)

        sum_diagnosability = 0
        for k_group in range(len(divider)):
            i_passed_spectra = passed_spectra[choosed_index[k_group][0]]
            i_failed_spectra = failed_spectra[choosed_index[k_group][1]]
            i_spectra = np.concatenate((i_passed_spectra, i_failed_spectra), axis=1)
            sum_diagnosability += uniqueness(i_spectra)
        average_diagnosability = sum_diagnosability / len(divider)
        return average_num, average_diagnosability

    def divide_func(optimal_min_num):
        optimal_min_num = int(optimal_min_num)
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            magnification_factor = 2  # 放大系数由清理效果决定，若保守估计清理效果有50%，则放大系数可定为2
            passed_group_num = magnification_factor * failed_group_num if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        # passed_list = aug_list(passed_list, len(failed_list))
        # failed_list = aug_list(failed_list, len(failed_list))

        # choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)
        choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
        return average_num, divider

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    min_divide_num = 8
    max_divide_num = 12
    min_minority_num = math.ceil((min(len(passed_remained_index), len(failed_remained_index)) + 1) / 2)
    if min_minority_num <= min_divide_num:
        the_divide_num = min_minority_num
    else:
        if min_minority_num <= max_divide_num:
            bin_num_list = [i for i in range(1, min_minority_num + 1)]
        else:
            bin_num_list = [i for i in range(1, max_divide_num + 1)]
            bin_num_list.append(min_minority_num)

        curve_x = []
        curve_y = []
        for the_num in bin_num_list:
            true_num, ratio = min_divide_ratio_func(the_num)
            curve_x.append(true_num)
            curve_y.append(ratio)
        the_divide_num = find_divide_num_using_difference(curve_x, curve_y, min_divide_num, max_divide_num)
    true_num, divider = divide_func(the_divide_num)
    print('拆分个数:{}'.format(true_num))
    return divider


def group_fix_divider(hardness, X, y):
    def min_divide_ratio_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
            passed_group_num = imbalanced_ratio * failed_group_num  # if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
            failed_group_num = imbalanced_ratio * passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        failed_spectra = list2singlespectra(failed_list, X_failed)
        passed_spectra = list2singlespectra(passed_list, X_passed)

        choosed_index, divider = simple_integrate(y_passed, y_failed, passed_list, failed_list, 100)

        sum_diagnosability = 0
        for k_group in range(len(divider)):
            i_passed_spectra = passed_spectra[choosed_index[k_group][0]]
            i_failed_spectra = failed_spectra[choosed_index[k_group][1]]
            i_spectra = np.concatenate((i_passed_spectra, i_failed_spectra), axis=1)
            sum_diagnosability += uniqueness(i_spectra)
        average_diagnosability = sum_diagnosability / len(divider)
        return average_num, average_diagnosability

    def divide_func(optimal_min_num):
        optimal_min_num = int(optimal_min_num)
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            magnification_factor = 2  # 放大系数由清理效果决定，若保守估计清理效果有50%，则放大系数可定为2
            passed_group_num = magnification_factor * failed_group_num if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        # passed_list = aug_list(passed_list, len(failed_list))
        # failed_list = aug_list(failed_list, len(failed_list))

        # choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list)
        choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
        return average_num, divider

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    optimal_divide_num = math.ceil(math.log10(len(X[0])) / math.log10(2))
    min_minority_num = math.ceil((min(len(passed_remained_index), len(failed_remained_index)) + 1) / 2)
    if min_minority_num <= optimal_divide_num:
        the_divide_num = min_minority_num
    else:
        bin_num_list = [1, optimal_divide_num, min_minority_num]
        curve_x = []
        curve_y = []
        for the_num in bin_num_list:
            true_num, ratio = min_divide_ratio_func(the_num)
            curve_x.append(true_num)
            curve_y.append(ratio)
        the_divide_num = find_fixed_divide_num(curve_x, curve_y)
    true_num, divider = divide_func(the_divide_num)
    print('拆分个数:{}'.format(true_num))
    return divider


def group_failed_uniqueness_difference_divider_backup(hardness, X, y):
    def divide_ratio_func(optimal_min_num):
        # if len(failed_remained_index) <= len(passed_remained_index):
        #     failed_num = min(optimal_min_num, len(failed_remained_index))
        #     failed_group_num = max(1, len(failed_remained_index) // failed_num)
        #     passed_group_num = 2 * failed_group_num if failed_group_num != 1 else 1
        #     passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        # else:
        #     passed_num = min(optimal_min_num, len(passed_remained_index))
        #     passed_group_num = max(1, len(passed_remained_index) // passed_num)
        #     failed_group_num = max(1, passed_group_num // 2)
        #     failed_num = max(1, len(failed_remained_index) // failed_group_num)
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
            passed_group_num = imbalanced_ratio * failed_group_num  # if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
            failed_group_num = imbalanced_ratio * passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        if len(failed_remained_index) <= len(passed_remained_index):
            sum_failed_diagnosability = 0
            for i in range(len(failed_list)):
                failed_index = failed_list[i]
                i_X_failed = X_failed[failed_index]
                sum_failed_diagnosability += DDU(i_X_failed)
            average_num = len(failed_remained_index) / len(failed_list)
            average_diagnosability = sum_failed_diagnosability / len(failed_list)
        else:
            sum_passed_diagnosability = 0
            for i in range(len(passed_list)):
                passed_index = passed_list[i]
                i_X_passed = X_passed[passed_index]
                sum_passed_diagnosability += DDU(i_X_passed)
            average_num = len(passed_remained_index) / len(passed_list)
            average_diagnosability = sum_passed_diagnosability / len(passed_list)

        # aug_passed_list = aug_list(passed_list, len(passed_list))
        # aug_failed_list = aug_list(failed_list, len(passed_list))

        choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
        return average_num, average_diagnosability, divider

    def binary_search(lowerbound, upperbound, ratio):
        if lowerbound >= upperbound:
            the_num, the_ratio, divider = divide_ratio_func(lowerbound)
            return lowerbound, divider
        mid_num = int((upperbound + lowerbound) / 2)
        # # the_ratio = slope_func(mid_num)
        the_num, the_ratio, divider = divide_ratio_func(mid_num)
        if mid_num == lowerbound:
            if the_ratio >= ratio:
                return mid_num, divider
            else:
                the_num, the_ratio, divider = divide_ratio_func(upperbound)
                return upperbound, divider
        elif the_ratio == ratio:
            return mid_num, divider
        elif the_ratio > ratio:
            return binary_search(lowerbound, mid_num, ratio)
        else:
            return binary_search(mid_num, upperbound, ratio)

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    # X_failed_remained = X_failed[failed_remained_index]
    # X_passed_remained = X_passed[passed_remained_index]
    # random.shuffle(X_failed_remained)
    # random.shuffle(X_passed_remained)

    # X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    # X_remained = X[X_remained_index]
    # the_diagnosability = DDU(X_remained)
    min_minority_num = min(len(passed_remained_index), len(failed_remained_index))

    curve_x = []
    curve_y = []
    curve_divide = []
    for the_num in range(1, min_minority_num + 1):
        true_num, ratio, divider = divide_ratio_func(the_num)
        curve_x.append(true_num)
        curve_y.append(ratio)
        curve_divide.append(divider)
    the_minority_num, divider = find_divide_num(curve_x, curve_y, curve_divide)

    return divider


def group_failed_uniqueness_difference_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    X_failed_remained = X_failed[failed_remained_index]
    X_passed_remained = X_passed[passed_remained_index]

    # X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    # X_remained = X[X_remained_index]
    # y_remained = y[X_remained_index]
    X_passed_single, index_passed = np.unique(X_passed_remained, axis=0, return_index=True)
    X_failed_single, index_failed = np.unique(X_failed_remained, axis=0, return_index=True)

    failed_single_index = np.array([failed_remained_index[index] for index in index_failed])
    passed_single_index = np.array([passed_remained_index[index] for index in index_passed])
    hardness_failed_single = np.array([hardness_failed_remained[index] for index in index_failed])
    hardness_passed_single = np.array([hardness_passed_remained[index] for index in index_passed])

    # the_diagnosability = DDU(X_remained)
    min_minority_num = math.ceil((min(len(X_passed_single), len(X_failed_single)) + 1) / 2)

    curve_x = []
    curve_y = []
    for the_num in range(1, min_minority_num + 1):
        true_num, ratio = outside_divide_ratio_func(the_num, failed_single_index, passed_single_index,
                                                    hardness_failed_single, hardness_passed_single, X_failed, X_passed)
        curve_x.append(true_num)
        curve_y.append(ratio)
    the_minority_num = find_divide_num(curve_x, curve_y)
    all_single_ratio = len(X_passed_remained) / len(X_passed_single) if len(X_passed_single) < len(
        X_failed_single) else len(X_failed_remained) / len(X_failed_single)
    all_divide_num = all_single_ratio * the_minority_num
    true_divide_num, divider = outside_divide_func(all_divide_num, failed_remained_index, passed_remained_index,
                                                   hardness_failed_remained, hardness_passed_remained, y_failed,
                                                   y_passed)

    return divider


def group_fixed_length_divider(hardness, X, y, itimes):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    X_failed_remained = X_failed[failed_remained_index]
    X_passed_remained = X_passed[passed_remained_index]

    X_passed_single, index_passed = np.unique(X_passed_remained, axis=0, return_index=True)
    X_failed_single, index_failed = np.unique(X_failed_remained, axis=0, return_index=True)

    # failed_single_index = np.array([failed_remained_index[index] for index in index_failed])
    # passed_single_index = np.array([passed_remained_index[index] for index in index_passed])
    # hardness_failed_single = np.array([hardness_failed_remained[index] for index in index_failed])
    # hardness_passed_single = np.array([hardness_passed_remained[index] for index in index_passed])
    #
    # min_minority_num = math.ceil((min(len(X_passed_single), len(X_failed_single)) + 1) / 2)
    #
    # curve_x = []
    # curve_y = []
    # for the_num in range(1, min_minority_num + 1):
    #     true_num, ratio = outside_divide_ratio_func(the_num, failed_single_index, passed_single_index, hardness_failed_single, hardness_passed_single, X_failed, X_passed)
    #     curve_x.append(true_num)
    #     curve_y.append(ratio)
    # the_minority_num = find_divide_num(curve_x, curve_y)
    all_single_ratio = len(X_passed_remained) / len(X_passed_single) if len(X_passed_single) < len(
        X_failed_single) else len(X_failed_remained) / len(X_failed_single)
    min_divide_num = math.ceil(all_single_ratio * max(math.log10(len(X[0])) / (math.log10(2) * 2), len(X[0]) / 500))
    # group_num = min(10, max(1, math.floor(len(failed_remained_index) / min_divide_num)))
    # original_min_num = len(hardness_failed_remained) + len(X_passed) - len(hardness_passed_remained) if len(X_failed_remained) < len(X_passed_remained) else len(hardness_passed_remained) + len(X_failed) - len(hardness_failed_remained)
    # if len(failed_remained_index) < len(passed_remained_index):
    #     original_failed_num = 0.5 * (len(X_passed) + 2 * len(X_failed_remained)) - 0.5 * math.sqrt(
    #         len(X_passed) ^ 2 - 4 * (len(X_failed) - len(failed_remained_index)) * len(failed_remained_index))
    # else:
    #     original_passed_num = 0.5 * (len(X_passed) + 2 * (len(X_failed) - len(X_failed_remained))) + 0.5 * math.sqrt(
    #         len(X_passed) ^ 2 - 4 * (len(X_failed) - len(X_failed_remained)) * len(X_failed_remained))

    # all_divide_num = math.ceil(2 * all_single_ratio * max(math.log10(len(X[0])) / (math.log10(2)), len(X[0]) / 250) / (len(X_passed_remained) + len(X_failed_remained)) * min(len(X_passed_remained), len(X_failed_remained)))

    # min_group_num = max(1, original_min_num // all_divide_num)
    # all_divide_num = math.ceil(all_single_ratio * max(math.log10(len(X[0])) / (math.log10(2)), len(X[0]) / 500))
    # all_divide_num = 10
    # all_divide_num = math.ceil(min(len(X_passed_remained), len(X_failed_remained)) / 6)
    # divider = outside_divide_func_robust(min_group_num, failed_remained_index, passed_remained_index,
    #                                      hardness_failed_remained, hardness_passed_remained, X_failed_remained,
    #                                      X_passed_remained, y_failed, y_passed, itimes)
    divider = outside_divide_func2(min_divide_num, failed_remained_index, passed_remained_index, hardness_failed_remained, hardness_passed_remained, y_failed, y_passed, itimes)
    # group_num = 3
    # divider = outside_divide_func_group(group_num, failed_remained_index, passed_remained_index, hardness_failed_remained, hardness_passed_remained, y_failed, y_passed, itimes)

    return divider


def group_failed_uniqueness_divider(hardness, X, y):
    def average_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            passed_group_num = 2 * failed_group_num
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = max(1, passed_group_num // 2)
            failed_num = max(1, len(failed_remained_index) // failed_group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        if len(failed_remained_index) <= len(passed_remained_index):
            sum_failed_diagnosability = 0
            for i in range(len(failed_list)):
                failed_index = failed_list[i]
                i_X_failed = X_failed[failed_index]
                sum_failed_diagnosability += DDU(i_X_failed)
            min_diagnosability = sum_failed_diagnosability / len(failed_remained_index)
        else:
            sum_passed_diagnosability = 0
            for i in range(len(passed_list)):
                passed_index = passed_list[i]
                i_X_passed = X_passed[passed_index]
                sum_passed_diagnosability += DDU(i_X_passed)
            min_diagnosability = sum_passed_diagnosability / len(passed_remained_index)
        return min_diagnosability

    def divide_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            passed_group_num = 2 * failed_group_num
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = max(1, passed_group_num // 2)
            failed_num = max(1, len(failed_remained_index) // failed_group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)
        divider = integrate(y_passed, y_failed, passed_list, failed_list)
        return divider

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    X_failed_remained = X_failed[failed_remained_index]
    X_passed_remained = X_passed[passed_remained_index]
    random.shuffle(X_failed_remained)
    random.shuffle(X_passed_remained)

    X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    X_remained = X[X_remained_index]
    min_minority_num = math.ceil(min(len(passed_remained_index), len(failed_remained_index)) / 2)
    # min_minority_num = min(len(passed_remained_index), len(failed_remained_index))
    curve_y = []
    for the_num in range(1, min_minority_num + 2):
        curve_y.append(average_func(the_num))

    max_diag = max(curve_y)
    max_index = curve_y.index(max_diag)
    divide_num = max_index
    curve_y_right = curve_y[max_index:]
    for index in range(len(curve_y_right)):
        if curve_y_right[index] >= 0.6 * max_diag:
            divide_num = max_index + index + 2

    divider = divide_func(divide_num)
    return divider


def group_failed_uniqueness_try(hardness, X, y):
    def average_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            imbalanced_ratio = len(passed_remained_index) // len(failed_remained_index)
            passed_group_num = imbalanced_ratio * failed_group_num  # if failed_group_num != 1 else 1
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
            average_num = len(failed_remained_index) / failed_group_num
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            imbalanced_ratio = len(failed_remained_index) // len(passed_remained_index)
            failed_group_num = imbalanced_ratio * passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)
            average_num = len(passed_remained_index) / passed_group_num

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)

        failed_spectra = list2singlespectra(failed_list, X_failed)
        passed_spectra = list2singlespectra(passed_list, X_passed)

        sum_diagnosability = 0
        if len(failed_remained_index) <= len(passed_remained_index):
            for k_group in range(len(failed_spectra)):
                i_failed_spectra = failed_spectra[k_group]
                sum_diagnosability += uniqueness(i_failed_spectra)
            average_diagnosability = sum_diagnosability / len(failed_spectra)
        else:
            for k_group in range(len(passed_spectra)):
                i_passed_spectra = passed_spectra[k_group]
                sum_diagnosability += uniqueness(i_passed_spectra)
            average_diagnosability = sum_diagnosability / len(passed_spectra)

        # if len(failed_remained_index) <= len(passed_remained_index):
        #     sum_failed_diagnosability = 0
        #     for i in range(len(failed_list)):
        #         failed_index = failed_list[i]
        #         i_X_failed = X_failed[failed_index]
        #         sum_failed_diagnosability += DDU(i_X_failed)
        #     average_diagnosability = sum_failed_diagnosability / len(failed_list)
        # else:
        #     sum_passed_diagnosability = 0
        #     for i in range(len(passed_list)):
        #         passed_index = passed_list[i]
        #         i_X_passed = X_passed[passed_index]
        #         sum_passed_diagnosability += DDU(i_X_passed)
        #     average_diagnosability = sum_passed_diagnosability / len(passed_list)
        return average_diagnosability, average_num

    def divide_func(optimal_min_num):
        if len(failed_remained_index) <= len(passed_remained_index):
            failed_num = min(optimal_min_num, len(failed_remained_index))
            failed_group_num = max(1, len(failed_remained_index) // failed_num)
            passed_group_num = 2 * failed_group_num
            passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        else:
            passed_num = min(optimal_min_num, len(passed_remained_index))
            passed_group_num = max(1, len(passed_remained_index) // passed_num)
            failed_group_num = passed_group_num
            failed_num = max(1, len(failed_remained_index) // failed_group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

        failed_list = divider2list(failed_remained_index, failed_divider)
        passed_list = divider2list(passed_remained_index, passed_divider)
        choosed_index, divider = part_integrate(y_passed, y_failed, passed_list, failed_list, 800)
        return divider

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    # random.shuffle(X_failed_remained)
    # random.shuffle(X_passed_remained)

    # X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    # X_remained = X[X_remained_index]
    min_minority_num = math.ceil((min(len(passed_remained_index), len(failed_remained_index)) + 1) / 2)

    the_diagnosability, all_num = average_func(min_minority_num)

    diagnosability, true_num = average_func(6)
    ratio = diagnosability / the_diagnosability
    divider = divide_func(min_minority_num)
    return divider


def group_one_uniqueness_divider(hardness, X, y):
    def average_func(X_one, hardness_one_remained, one_remained_index, optimal_min_num, the_diag):
        one_num = min(optimal_min_num, len(one_remained_index))
        one_divider = divide_base(hardness_one_remained, one_num, 'random')
        one_list = divider2list(one_remained_index, one_divider)

        sum_one_diagnosability = 0
        for i in range(len(one_list)):
            one_index = one_list[i]
            i_X_one = X_one[one_index]
            sum_one_diagnosability += DDU(i_X_one)
        average_diagnosability = sum_one_diagnosability / len(one_list)

        uni_ratio = 1 - average_diagnosability / the_diag
        num_ratio = 1 - 1 / len(one_list)
        ratio = uni_ratio / num_ratio if num_ratio != 0 else 0.
        return ratio, one_list

    # def find_divide_num(curve, curve_divider, bin_num, min_num=0):
    #     divide_num = bin_num[0]
    #     divide_list = curve_divider[0]
    #     for diag in curve:
    #         if diag <= 0.4:
    #             index = curve.index(diag)
    #             if bin_num[index] > min_num:
    #                 divide_num = bin_num[index]
    #                 divide_list = curve_divider[index]
    #             else:
    #                 break
    #     return divide_num, divide_list

    # def find_divide_num(curve, bin_num):
    #     max_diag = max(curve)
    #     max_index = curve.index(max_diag)
    #     divide_num = bin_num[max_index]
    #     curve_right = curve[max_index:]
    #     for index in range(len(curve_right)):
    #         if curve_right[index] >= 0.5 * max_diag:
    #             divide_num = bin_num[max_index + index + 1] if max_index + index + 1 < len(bin_num) else bin_num[-1]
    #     return divide_num

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])
    X_failed_remained = X_failed[failed_remained_index]
    X_passed_remained = X_passed[passed_remained_index]

    failed_diagnosability = DDU(X_failed_remained)
    passed_diagnosability = DDU(X_passed_remained)

    bins_num = 20
    bin_failed_num = bin_func(math.ceil(len(failed_remained_index) / 2) + 1, bins_num)
    bin_passed_num = bin_func(math.ceil(len(passed_remained_index) / 2) + 1, bins_num)
    reversed_bin_failed_num = bin_failed_num[::-1]
    reversed_bin_passed_num = bin_passed_num[::-1]
    curve_x = []
    curve_x_divider = []
    curve_y = []
    curve_y_divider = []
    for the_num in reversed_bin_failed_num:
        ratio, failed_list = average_func(X_failed, hardness_failed_remained, failed_remained_index, the_num,
                                          failed_diagnosability)
        curve_x.append(ratio)
        curve_x_divider.append(failed_list)

    for the_num in reversed_bin_passed_num:
        ratio, passed_list = average_func(X_passed, hardness_passed_remained, passed_remained_index, the_num,
                                          passed_diagnosability)
        curve_y.append(ratio)
        curve_y_divider.append(passed_list)

    passed_divide_num, passed_list = find_divide_num(curve_y, curve_y_divider, reversed_bin_passed_num)
    min_failed_num = len(hardness_failed_remained) / (len(passed_list) + 1) + 1
    failed_divide_num, failed_list = find_divide_num(curve_x, curve_x_divider, reversed_bin_failed_num, min_failed_num)

    aug_failed_list = aug_list(failed_list, len(failed_list))
    aug_passed_list = aug_list(passed_list, len(passed_list))

    divider = integrate(y_passed, y_failed, aug_passed_list, aug_failed_list)
    return divider


def group_max_ddu_divider(hardness, X, y):
    def divide_func(optimal_min_num):
        # if len(failed_remained_index) <= len(passed_remained_index):
        #     optimal_failed_num = max(3, round(X.shape[1] / 500))
        #     # optimal_failed_num = 5
        #     failed_num = min(optimal_failed_num, len(failed_remained_index))
        #     failed_group_num = max(1, len(failed_remained_index) // failed_num)
        #     passed_group_num = failed_group_num
        #     passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        #
        # else:
        #     # optimal_passed_num = max(3, round(X.shape[1] / 500))
        #     optimal_passed_num = 20
        #     passed_num = min(optimal_passed_num, len(passed_remained_index))
        #     passed_group_num = max(1, len(passed_remained_index) // passed_num)
        #     failed_group_num = max(1, passed_group_num // 4)
        #     failed_num = max(1, len(failed_remained_index) // failed_group_num)

        group_num = max(1, math.floor(min(len(passed_remained_index), len(failed_remained_index)) / optimal_min_num))
        failed_num = math.floor(len(failed_remained_index) / group_num)
        passed_num = math.floor(len(passed_remained_index) / group_num)

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'nonuniform')
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'nonuniform')

        divider = []
        sum_diagnosability = 0
        for i_group in range(group_num):
            passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
            failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])
            X_indexes = np.hstack((passed_indexes, len(X_passed) + failed_indexes))
            i_X = X[X_indexes]
            sum_diagnosability += DDU(i_X)
            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
            divider.append(two_indexes)
        average_diagnosability = sum_diagnosability / min(len(passed_remained_index), len(failed_remained_index))
        diagnosability_ratio = average_diagnosability / the_diagnosability
        return diagnosability_ratio, divider

    def binary_search(lowerbound, upperbound, ratio):
        if lowerbound >= upperbound:
            the_ratio, divider = divide_func(lowerbound)
            return lowerbound, divider
        mid_num = int((upperbound + lowerbound) / 2)
        the_ratio, divider = divide_func(mid_num)
        if mid_num == lowerbound:
            if the_ratio <= ratio:
                return mid_num, divider
            else:
                the_ratio, divider = divide_func(upperbound)
                return upperbound, divider
        elif the_ratio == ratio:
            return mid_num, divider
        elif the_ratio < ratio:
            return binary_search(lowerbound, mid_num, ratio)
        else:
            return binary_search(mid_num, upperbound, ratio)

    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    X_remained_index = np.hstack((passed_remained_index, len(X_passed) + failed_remained_index))
    X_remained = X[X_remained_index]
    the_diagnosability = DDU(X_remained)
    max_group_num = min(len(passed_remained_index), len(failed_remained_index))
    # final_group_num, divider = binary_search(3, max_group_num, 0.1)
    the_ratio, divider = divide_func(8)
    return divider


def group_max_cross_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    # optimal_failed_num = round(30 * math.sqrt(len(failed_remained_index)/(len(passed_remained_index) + len(failed_remained_index))))
    # optimal_failed_num = 40
    optimal_failed_num = max(3, X.shape[1] / 500)
    # group_num = max(1, math.floor(len(failed_remained_index) / optimal_failed_num))
    group_num = 6
    failed_num = math.floor(len(failed_remained_index) / group_num)
    passed_num = math.floor(len(passed_remained_index) / (group_num))

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    # passed_divider = no_divide(hardness_passed)
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    passed_list = []
    failed_list = []
    for k_group in range(len(failed_divider)):
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[k_group]])
        failed_list.append(failed_indexes)
    for m_group in range(len(passed_divider)):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[m_group]])
        passed_list.append(passed_indexes)

    for j_group in range(len(failed_list)):
        chosed_passed_list_index = random.sample(range(len(passed_list)), math.floor(math.sqrt(len(passed_list))))
        for i_group in chosed_passed_list_index:
            two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
            divider.append(two_indexes)
    return divider


def group_unsample_cross_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    # group_num = min(5, max(1, math.floor(min(len(passed_remained_index), len(failed_remained_index)) / 5)))
    group_num = 1
    failed_num = max(1, math.floor(2 / 3 * len(failed_remained_index)))
    passed_num = math.floor(len(passed_remained_index) / group_num)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    failed_divider = under_sampling_base(hardness_failed_remained, failed_num, group_num)
    passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    passed_list = []
    failed_list = []
    for i_group in range(group_num):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])
        # failed_indexes = random_under_sampling(X_failed, group_len)
        passed_list.append(passed_indexes)
        failed_list.append(failed_indexes)
    for i_group in range(len(passed_list)):
        for j_group in range(len(failed_list)):
            two_indexes = {str(y_passed[0]): passed_list[i_group], str(y_failed[0]): failed_list[j_group]}
            divider.append(two_indexes)
    return divider


def group_unsample_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    sample_num = max(1, math.ceil(2 / 3 * len(failed_remained_index)))

    # passed_divider = no_divide(hardness_passed)
    passed_divider = divide_base(hardness_passed_remained, sample_num)
    # passed_divider = cluster_divide(X_passed, len(failed_unsampled))

    for i_group in range(len(passed_divider)):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])

        failed_unsampled = random_under_sampling(hardness_failed_remained, sample_num)
        failed_indexes = np.array([failed_remained_index[index] for index in failed_unsampled])

        two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
        divider.append(two_indexes)
    return divider


def group_passed_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 根据model对多数类进行拆分
    divider = []
    if len(X_passed) >= len(X_failed):
        # 扔掉一些难分类数据
        failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
        passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
        hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
        hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

        # group_num = min(10, math.ceil(len(passed_remained_index) / len(failed_remained_index)))
        # passed_num = math.floor(len(passed_remained_index) / group_num)
        passed_num = max(min(1, len(passed_remained_index)), len(failed_remained_index))

        # passed_divider = no_divide(hardness_passed)
        passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')
        # passed_divider = cluster_divide(X_passed, len(failed_unsampled))

        for i_group in range(len(passed_divider)):
            passed_indexes = np.array([passed_remained_index[index] for index in passed_divider[i_group]])
            failed_indexes = np.array(failed_remained_index)

            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
            divider.append(two_indexes)
    else:
        # 扔掉一些难分类数据
        failed_remained_index = drop_hard_data(hardness_failed, 1.1, least_num=1)
        passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
        hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
        hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

        # group_num = min(10, math.ceil(len(failed_remained_index) / len(passed_remained_index)))
        # failed_num = math.floor(len(failed_remained_index) / group_num)
        failed_num = max(min(1, len(failed_remained_index)), len(passed_remained_index))

        failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
        for i_group in range(len(failed_divider)):
            passed_indexes = np.array(passed_remained_index)
            failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])

            two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
            divider.append(two_indexes)

    return divider


def group_failed_divider(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    group_num = min(5, max(1, math.floor(min(len(passed_remained_index), len(failed_remained_index)) / 5)))
    failed_num = math.floor(len(failed_remained_index) / group_num)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    failed_divider = divide_base(hardness_failed_remained, failed_num, 'random')
    passed_divider = no_divide(hardness_passed_remained)
    # passed_divider = divide_base(hardness_passed_remained, passed_num, 'random')

    # 根据model对passed进行拆分，对failed进行欠采样
    divider = []
    for i_group in range(group_num):
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider])
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[i_group]])
        # failed_indexes = random_under_sampling(X_failed, group_len)

        two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
        divider.append(two_indexes)
    return divider


def group_unsampled(hardness, X, y):
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    # 扔掉一些难分类数据
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    group_num = max(10, math.floor(min(len(passed_remained_index), len(failed_remained_index)) / 3))

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])
    failed_sample_num = 3
    passed_sample_num = round(failed_sample_num * len(passed_remained_index) / len(failed_remained_index))

    # 根据model 进行n次分组
    divider = []
    for i_sampling in range(group_num):
        failed_divider = random_under_sampling(hardness_failed_remained, failed_sample_num)
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider])

        passed_divider = random_under_sampling(hardness_passed_remained, passed_sample_num)
        passed_indexes = np.array([passed_remained_index[index] for index in passed_divider])
        two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
        divider.append(two_indexes)

    return divider


def group_minimal_clean(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    divider = []
    if len(X_passed) >= len(X_failed):
        # 扔掉一些难分类数据
        failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
        passed_remained_index = drop_hard_data(hardness_passed, 2, least_num=1)

    else:
        failed_remained_index = drop_hard_data(hardness_failed, 2, least_num=1)
        passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)

    two_indexes = {str(y_passed[0]): passed_remained_index, str(y_failed[0]): failed_remained_index}
    divider.append(two_indexes)
    return divider


def group_clean(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    divider = []

    # if len(y_failed) >= len(y_passed):
    #     failed_remained_index = drop_hard_data(hardness_failed, 1.1, least_num=1)
    #     passed_remained_index = drop_hard_data(hardness_passed, 1, least_num=1)
    # else:
    #     failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    #     passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)
    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)

    passed_indexes = passed_remained_index
    failed_indexes = failed_remained_index

    two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
    divider.append(two_indexes)
    return divider


def group_enlarge(hardness, X, y, itimes):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    divider = []

    failed_remained_index = drop_hard_data(hardness_failed, 1.1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)

    hardness_failed_remained = np.array([hardness_failed[index] for index in failed_remained_index])
    hardness_passed_remained = np.array([hardness_passed[index] for index in passed_remained_index])

    confident_ratio = 0.1

    if len(failed_remained_index) <= len(passed_remained_index):
        first_failed, last_failed = distinguish_two_regions(hardness_failed_remained, confident_ratio)
        first_failed_index, last_failed_index = [failed_remained_index[index] for index in first_failed], [
            failed_remained_index[index] for index in last_failed]
        aug_ratio = (len(passed_remained_index) / len(failed_remained_index) + confident_ratio - 1) / confident_ratio
        aug_num = int(aug_ratio)
        aug_numerical = len(passed_remained_index) - len(first_failed_index) * aug_num - len(last_failed)
        random.seed(itimes)
        failed_list = first_failed_index * aug_num + [random.choice(first_failed_index) for _ in
                                                      range(aug_numerical)] + last_failed_index
        passed_list = passed_remained_index
    else:
        # first_passed, last_passed = distinguish_two_regions(hardness_passed_remained, confident_ratio)
        # first_passed_index, last_passed_index = [passed_remained_index[index] for index in first_passed], [passed_remained_index[index] for index in last_passed]
        # aug_ratio = (len(failed_remained_index) / len(passed_remained_index) + confident_ratio - 1) / confident_ratio
        # aug_num = int(aug_ratio)
        # aug_numerical = len(failed_remained_index) - len(first_passed_index) * aug_num - len(last_passed)
        # random.seed(itimes)
        # passed_list = first_passed_index * aug_num + [random.choice(first_passed_index) for _ in range(aug_numerical)] + last_passed_index
        # failed_list = failed_remained_index
        passed_list = passed_remained_index
        failed_list = failed_remained_index

    two_indexes = {str(y_passed[0]): np.array(passed_list), str(y_failed[0]): np.array(failed_list)}
    divider.append(two_indexes)
    return divider


if __name__ == "__main__":
    # a, b = num2group(8, 2, 3, 4)
    # print(a)
    # print(b)
    # a = bin_func(30, 1)
    # print(a)
    # x = [1,2,3,4,5,6]
    # y = [1,2,5,6,6.5,6.6]
    # print(is_decreasing_trend(x,y))

    # arr = np.array([[1, 2, 3], [4, 5, 6], [1, 2, 3], [7, 8, 9], [4, 5, 6]])
    # unique_arr, index = np.unique(arr, axis=0, return_index=True)
    # delete_index = np.setdiff1d(np.arange(len(arr)), index)

    # X = numpy.array([[0,1,1,0,1],[0,1,1,1,1],[1,1,1,1,0],[0,0,1,0,1],[0,0,0,0,0],[1,1,1,1,1]])
    #
    # y = divide_remainder(X, 12)

    import numpy as np
    import pandas as pd

    # # 假设A和B是我们的输入矩阵
    # A = np.array([[1, 2, 3], [4, 5, 6], [7, 8, 9], [1, 2, 3]])
    # B = np.array([[1, 2, 3], [10, 11, 12]])
    #
    # # 将A和B转化为pandas DataFrame
    # df_A = pd.DataFrame(A)
    # df_B = pd.DataFrame(B)
    #
    # # 将在A中也在B中的行从A中去重
    # df_A_unique = df_A.drop(df_A[df_A.isin(df_B)].index)
    #
    # # 将A转化回numpy数组
    # A_unique = df_A_unique.to_numpy()
    #
    # print(A_unique)

    # original_failed_num = 0.5 * (97 + 2 * 19) - 0.5 * math.sqrt(97 ^ 2 - 4 * (23 - 19) * 19)
    # original_passed_num = 0.5 * (23 + 2 * (97 - 96)) + 0.5 * math.sqrt(23 ^ 2 - 4 * (97 - 96) * 96)
    print(divide_and_resample([1,2,3,4,5,6,7,8,9,10], 3))
    sys.exit()
