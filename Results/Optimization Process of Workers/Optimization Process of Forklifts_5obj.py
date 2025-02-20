import matplotlib.pyplot as plt
import pandas as pd
from matplotlib.ticker import MultipleLocator

# 创建高分辨率的图形对象
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)

# 读取数据
file_path = 'Optimization_Process_5obj.csv'
data = pd.read_csv(file_path)


# 设置滑动窗口大小
n =160  # 可以根据需要调整这个值
colors=[[84/256,134/256,135/256], [71/256,51/256,53/256], [5/256,80/256,91/256]]

# 计算滑动平均
data['SMA_Forklifts'] = data[' Forklifts'].rolling(window=n, min_periods=1).mean()


# 绘制滑动平均曲线
line_Forklifts= ax.plot(data['Iterations'], data['SMA_Forklifts'], alpha=0.75, linewidth = 2.0)
#plt.fill_between(data['Iterations'], 0, data['SMA_Forklifts'], color=colors[0], alpha=0.75, linewidth=0, edgecolor='none')


# 设置图表标题和轴标签
plt.xlabel('Iterations', fontproperties={'family': 'serif','size': 10,'style': 'normal'},)
plt.ylabel('Quantity of Forklifts', fontproperties={'family': 'serif','size': 10,'style': 'normal'},)

# 添加虚线网格
plt.grid(True, linestyle='-.', alpha=0.3)

plt.xlim([0, max(data['Iterations'])])
plt.ylim([0, 250])

ax.xaxis.set_major_locator(MultipleLocator(1000))  # 每50个单位设置一个x轴主刻度
ax.yaxis.set_major_locator(MultipleLocator(50))  # 每10个单位设置一个y轴主刻度

plt.legend(prop={'family': 'serif','size': 10,'style': 'normal'}, frameon = False,bbox_to_anchor=(1, 0.5))
plt.tick_params(axis='both', labelsize=10)

# 定义字体属性字典
font_props = {'family': 'serif','size': 10,'style': 'normal'}
plt.legend(prop=font_props, frameon = False)

# 调整边距和布局
plt.subplots_adjust(left=0.10, right=0.95, top=0.95, bottom=0.15, wspace=0.01, hspace=0.01)

# 保存图像
plt.savefig('Optimization Process of Forklifts_5obj.png', dpi=800, pad_inches=0)

# 显示图形
plt.show()