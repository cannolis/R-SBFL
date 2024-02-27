import sys

import numpy as np
import math
from collections import Counter
import random
from copy import deepcopy



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

def initial(X, y, hardness):
    # 将X，y分解为 X_passed、X_failed和 y_passed、y_failed
    sorted_class_distr = sorted(Counter(y).items(), key=lambda d: d[1])
    label_failed, label_passed = 1, 0
    passed_index, failed_index = (y == label_passed), (y == label_failed)
    X_passed, y_passed = X[passed_index], y[passed_index]
    X_failed, y_failed = X[failed_index], y[failed_index]
    hardness_passed, hardness_failed = hardness[passed_index], hardness[failed_index]

    return X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed


def distinguish_regions(hardness, group_len):
    _hardness = deepcopy(hardness)
    hardness_index = np.arange(len(_hardness))

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
                n_divider_index = divide_nonuniform(hardness, group_len, random_state)
            elif method == 'gradient':
                n_divider_index = divide_gradient(hardness, group_len, random_state)
            else:
                raise Exception('不在定义好的方法列表中')
    else:
        n_divider_index = np.array([np.arange(len(hardness))])

    return n_divider_index

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

    hardness_index = [i for i in range(len(hardness))]
    hardness_index.sort(key=lambda x: hardness[x])
    n_divider_index = divide_and_resample(hardness_index, group_len, random_state)

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
    n_divider_index = divide_and_resample(hardness_index, group_len, random_state)
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


def divider2list(failed_remained_index, failed_divider):
    failed_list = []
    for k_group in range(len(failed_divider)):
        failed_indexes = np.array([failed_remained_index[index] for index in failed_divider[k_group]])
        failed_list.append(failed_indexes)
    return failed_list


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



def outside_divide_func2(optimal_min_num, failed_remained_index, passed_remained_index, hardness_failed_remained,
                         hardness_passed_remained, y_failed, y_passed, itimes):
    optimal_min_num = int(optimal_min_num)
    if len(failed_remained_index) <= len(passed_remained_index):
        failed_num = min(optimal_min_num, len(failed_remained_index))
        failed_group_num = max(1, len(failed_remained_index) // failed_num)
        imbalanced_ratio = 2
        passed_group_num = math.ceil(imbalanced_ratio * failed_group_num) if failed_group_num != 1 else 1
        passed_num = max(failed_num, len(passed_remained_index) // passed_group_num)
        if len(failed_remained_index) // failed_num > 1:

            failed_divider = divide_repeat(hardness_failed_remained, failed_num, 'random', itimes, 20)
            passed_divider = divide_repeat(hardness_passed_remained, passed_num, 'random', itimes, 20)
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

    choosed_index, divider = integrate(y_passed, y_failed, passed_list, failed_list, itimes)
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

    all_single_ratio = len(X_passed_remained) / len(X_passed_single) if len(X_passed_single) < len(
        X_failed_single) else len(X_failed_remained) / len(X_failed_single)
    min_divide_num = math.ceil(all_single_ratio * max(math.log10(len(X[0])) / (math.log10(2) * 2), len(X[0]) / 500))
    divider = outside_divide_func2(min_divide_num, failed_remained_index, passed_remained_index, hardness_failed_remained, hardness_passed_remained, y_failed, y_passed, itimes)
    return divider



def group_clean(hardness, X, y):
    # 初始化
    X_passed, X_failed, y_passed, y_failed, hardness_passed, hardness_failed = initial(X, y, hardness)

    divider = []

    failed_remained_index = drop_hard_data(hardness_failed, 1, least_num=1)
    passed_remained_index = drop_hard_data(hardness_passed, 1.1, least_num=1)

    passed_indexes = passed_remained_index
    failed_indexes = failed_remained_index

    two_indexes = {str(y_passed[0]): passed_indexes, str(y_failed[0]): failed_indexes}
    divider.append(two_indexes)
    return divider

