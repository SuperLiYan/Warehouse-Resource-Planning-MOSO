import pandas as pd
import matplotlib.pyplot as plt
import numpy as np
from array import array

def interleave_lists(forklifts_am, forklifts_pm):
    rate_forklifts_dock = []
    # Loop through the shortest list length to avoid index error
    for i in range(min(len(forklifts_am), len(forklifts_pm))):
        rate_forklifts_dock.append(forklifts_am[i])
        rate_forklifts_dock.append(forklifts_pm[i])
    return rate_forklifts_dock

# 读取数据
file_path = 'ManpowerShiftSchedule_5obj.csv'
df = pd.read_csv(file_path)
colors=[[84/256,134/256,135/256], [71/256,51/256,53/256], [5/256,80/256,91/256]]
# 设置参数

# 计算每班的工人数量
forklifts_dock_am = df['Dock_Forklift_am'].values
forklifts_inbound_am = df['In_Forklift_am'].values
forklifts_outbound_am = df['Out_Forklift_am'].values

forklifts_dock_pm = df['Dock_Forklift_pm'].values
forklifts_inbound_pm = df['In_Forklift_pm'].values
forklifts_outbound_pm = df['Out_Forklift_pm'].values


# forklifts_dock_am = df['forklift_dock_am'].values
# forklifts_inbound_am = df['forklift_inbound_am'].values
# forklifts_outbound_am = df['forklift_outbound_am'].values

# forklifts_dock_pm = df['forklift_dock_pm'].values
# forklifts_inbound_pm = df['forklift_inbound_pm'].values
# forklifts_outbound_pm = df['forklift_outbound_pm'].values

shift_forklifts_am = forklifts_dock_am + forklifts_inbound_am+ forklifts_outbound_am;
shift_forklifts_pm = forklifts_dock_pm + forklifts_inbound_pm+ forklifts_outbound_pm;



rate_forklifts_dock_am = forklifts_dock_am/shift_forklifts_am;
rate_forklifts_inbound_am = forklifts_inbound_am/shift_forklifts_am;
rate_forklifts_outbound_am = forklifts_outbound_am/shift_forklifts_am;

rate_forklifts_dock_pm = forklifts_dock_pm/shift_forklifts_pm;
rate_forklifts_inbound_pm = forklifts_inbound_pm/shift_forklifts_pm;
rate_forklifts_outbound_pm = forklifts_outbound_pm/shift_forklifts_pm;

rate_forklifts_dock=np.array(interleave_lists(rate_forklifts_dock_am, rate_forklifts_dock_am), dtype=np.float64)
rate_forklifts_inbound=np.array(interleave_lists(rate_forklifts_inbound_am, rate_forklifts_inbound_am), dtype=np.float64)
rate_forklifts_outbound=np.array(interleave_lists(rate_forklifts_outbound_am, rate_forklifts_outbound_am), dtype=np.float64)

# 创建图形
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)


# 填充gap区域
ax.fill_between(range(len(shift_forklifts_am)+len(shift_forklifts_pm)), 0, rate_forklifts_dock, step='post', alpha=0.3, color='red', label='Receiving dock', linewidth=0)
ax.fill_between(range(len(shift_forklifts_am)+len(shift_forklifts_pm)), rate_forklifts_dock, rate_forklifts_dock+rate_forklifts_inbound, step='post', alpha=0.4, color=colors[0], label='Inbound buffer area', linewidth=0)
ax.fill_between(range(len(shift_forklifts_am)+len(shift_forklifts_pm)), rate_forklifts_dock+rate_forklifts_inbound, rate_forklifts_dock+rate_forklifts_inbound+rate_forklifts_outbound , step='post', alpha=0.4, color=colors[1], label='Outbound buffer area', linewidth=0)

# 设置图表标题和轴标签
plt.grid(True, linestyle='-.', alpha=0.3)
plt.xlabel('Working days', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
plt.ylabel('Rate of forklifts allocated', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
ax.tick_params(axis='both', which='major', labelsize=8)

plt.xlim([0, len(shift_forklifts_am)+len(shift_forklifts_pm)-1])
plt.ylim([0, 1])

# 设置x轴刻度和标签
ticks = np.arange(0, len(shift_forklifts_am)+len(shift_forklifts_pm), 5)  # 每5天设置一个刻度
tick_labels = [f'Shift{i+1}' for i in ticks]  # 生成刻度标签列表
ax.set_xticks(ticks)  # 设置刻度位置
ax.set_xticklabels(tick_labels, rotation=0)  # 设置刻度标签，rotation=0使其水平显示

# 添加图例
font_props = {'family': 'serif', 'size': 6, 'style': 'normal'}
plt.legend(prop=font_props, frameon=False, loc='best')

# 显示图形
plt.tight_layout()  # 可以加上这个调整布局，以防止x轴标签被剪切
# 调整边距和布局
plt.subplots_adjust(left=0.10, right=0.95, top=0.95, bottom=0.15, wspace=0.01, hspace=0.01)

# 保存图像
plt.savefig('Rate of allocated forklifts_5obj.pdf', dpi=800, pad_inches=0)
plt.show()