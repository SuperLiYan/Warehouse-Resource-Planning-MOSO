import pandas as pd
import matplotlib.pyplot as plt
import numpy as np


# 读取数据
file_path = 'ManpowerShiftSchedule_5obj.csv'
df = pd.read_csv(file_path)
colors=[[84/256,134/256,135/256], [71/256,51/256,53/256], [5/256,80/256,91/256]]
# 设置参数
maximum_workers = 93

# 计算每班的工人数量
day_workers_forklift = df['Dock_staff_am'].values+df['Dock_staff_pm'].values+df['Out_Staff_Dri_am'].values+df['Out_Staff_Dri_pm'].values+df['In_Staff_Dri_am'].values+df['In_Staff_Dri_pm'].values
day_workers_unpack = day_workers_forklift + df['In_Staff_Unp_am'].values+df['In_Staff_Unp_pm'].values
day_workers_pack = day_workers_unpack + df['Out_Staff_Pa_am'].values+df['Out_Staff_Pa_pm'].values
day_workers = day_workers_pack

# 创建图形
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)

# 绘制最大工人数线
ax.axhline(maximum_workers, color='red', linewidth=1, label='Maximum workers', alpha=0.7)

# 填充gap区域
ax.fill_between(range(len(day_workers)), day_workers, maximum_workers, step='post', alpha=0.3, color='red', label='Unused workers', linewidth=0)
ax.fill_between(range(len(day_workers_forklift)), 0, day_workers_forklift, step='post', alpha=0.4, color=colors[0], label='Allocated workers for forklift operation', linewidth=0)
ax.fill_between(range(len(day_workers_unpack)), day_workers_forklift, day_workers_unpack, step='post', alpha=0.4, color=colors[1], label='Allocated workers for unpacking', linewidth=0)
ax.fill_between(range(len(day_workers_pack)), day_workers_unpack, day_workers_pack, step='post', alpha=0.4, color=colors[2], label='Allocated workers for unpacking', linewidth=0)

# 设置图表标题和轴标签
plt.grid(True, linestyle='-.', alpha=0.3)
plt.xlabel('Working days', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
plt.ylabel('Quantity of Workers', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
ax.tick_params(axis='both', which='major', labelsize=8)

plt.xlim([0, len(day_workers)-1])
plt.ylim([20, 100])

# 设置x轴刻度和标签
ticks = np.arange(0, len(day_workers), 5)  # 每5天设置一个刻度
tick_labels = [f'Day{i+1}' for i in ticks]  # 生成刻度标签列表
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
plt.savefig('Unused Workers_5obj.pdf', dpi=800, pad_inches=0)
plt.show()