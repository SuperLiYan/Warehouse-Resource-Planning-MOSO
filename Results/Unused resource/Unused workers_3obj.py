import pandas as pd
import matplotlib.pyplot as plt
import numpy as np


# 读取数据
file_path = 'ManpowerShiftSchedule_3obj.csv'
df = pd.read_csv(file_path)

# 设置参数
maximum_workers = 35

# 计算每班的工人数量
day_workers = df['worker_dock_am'].values + df['worker_inbound_am'].values + df['worker_outbound_am'].values + df['worker_dock_pm'].values + df['worker_inbound_pm'].values + df['worker_outbound_pm'].values

# 创建图形
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)

# 绘制最大工人数线
ax.axhline(maximum_workers, color='red', linewidth=1, label='Maximum workers', alpha=0.7)

# 填充gap区域
ax.fill_between(range(len(day_workers)), day_workers, maximum_workers, step='post', alpha=0.3, color='red', label='Unused workers')
ax.fill_between(range(len(day_workers)), 0, day_workers, step='post', alpha=0.3, color='blue', label='Allocated workers')

# 设置图表标题和轴标签
plt.grid(True, linestyle='-.', alpha=0.3)
plt.xlabel('Working days', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
plt.ylabel('Quantity of Workers', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
ax.tick_params(axis='both', which='major', labelsize=8)

plt.xlim([0, len(day_workers)-1])
plt.ylim([20, 36])

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
plt.savefig('Unused Workers_3obj.pdf', dpi=800, pad_inches=0)
plt.show()
