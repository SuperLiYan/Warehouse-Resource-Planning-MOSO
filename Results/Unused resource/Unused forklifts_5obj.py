import pandas as pd
import matplotlib.pyplot as plt
import numpy as np


# 读取数据
file_path = 'ManpowerShiftSchedule_5obj.csv'
df = pd.read_csv(file_path)

# 设置参数
maximum_forklifts = 21

# 计算每班的工人数量
shift_forklifts_am = df['Dock_Forklift_am'].values + df['In_Forklift_am'].values + df['Out_Forklift_am'].values
shift_forklifts_pm = df['Dock_Forklift_pm'].values + df['In_Forklift_pm'].values + df['Out_Forklift_pm'].values

# 创建图形
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)

# 绘制最大工人数线
ax.axhline(maximum_forklifts, color='red', linewidth=1, label='Maximum forklifts', alpha=0.7)
shift_forklifts = np.dstack((shift_forklifts_am, shift_forklifts_pm)).flatten()

# 填充gap区域
ax.fill_between(range(len(shift_forklifts)), shift_forklifts, maximum_forklifts, step='post', alpha=0.3, color='red', label='Unused forklifts')
ax.fill_between(range(len(shift_forklifts)), 0, shift_forklifts, step='post', alpha=0.3, color='blue', label='Allocated forklifts')

# 设置图表标题和轴标签
plt.grid(True, linestyle='-.', alpha=0.3)
plt.xlabel('Working shifts', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
plt.ylabel('Quantity of forklifts', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'},)
ax.tick_params(axis='both', which='major', labelsize=8)

plt.xlim([0, len(shift_forklifts)-1])
plt.ylim([0, 25])

# 设置x轴刻度和标签
ticks = np.arange(0, len(shift_forklifts), 5)  # 每5天设置一个刻度
tick_labels = [f'shift{i+1}' for i in ticks]  # 生成刻度标签列表
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
plt.savefig('Unused forklifts_5obj.pdf', dpi=800, pad_inches=0)
plt.show()