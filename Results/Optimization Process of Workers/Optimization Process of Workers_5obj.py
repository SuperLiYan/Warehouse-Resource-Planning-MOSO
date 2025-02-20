import matplotlib.pyplot as plt
import pandas as pd
from matplotlib.ticker import MultipleLocator

# 创建高分辨率的图形对象
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)

# 读取数据
file_path = 'Optimization_Process_5obj.csv'
data = pd.read_csv(file_path)


# 设置滑动窗口大小
n =100  # 可以根据需要调整这个值
colors=[[84/256,134/256,135/256], [71/256,51/256,53/256], [5/256,80/256,91/256]]

# 计算滑动平均
data['SMA_ManpowerofForklifts'] = data[' ManpowerofForklifts'].rolling(window=n, min_periods=1).mean()
data['SMA_ManpowerofUnpacking'] = data[' ManpowerofUnpacking'].rolling(window=n, min_periods=1).mean()
data['SMA_ManpowerofPacking'] = data[' ManpowerofPacking'].rolling(window=n, min_periods=1).mean()
# 计算滑动平均
data['SMA_Forklifts'] = data[' Forklifts'].rolling(window=n, min_periods=1).mean()


Cumulative_Time1 = data['SMA_ManpowerofForklifts']
Cumulative_Time2 = data['SMA_ManpowerofForklifts'] + data['SMA_ManpowerofUnpacking']
Cumulative_Time3 = data['SMA_ManpowerofForklifts'] + data['SMA_ManpowerofUnpacking'] + data['SMA_ManpowerofPacking']

# 绘制滑动平均曲线
line_Manpower= ax.plot(data['Iterations'], Cumulative_Time3, c="red", alpha=0.75, linewidth = 1.2, label='Total Workers')
line_Forklifts= ax.plot(data['Iterations'], data['SMA_Forklifts'], alpha=0.75, linewidth = 2.0, label='Forklifts')
#line_ManpowerofForklifts= ax.plot(data['Iterations'], data['SMA_ManpowerofForklifts'], label='AQT at the receiving dock')
#line_ ManpowerofUnpacking= ax.plot(data['Iterations'], data['SMA_ ManpowerofUnpacking'], label='AQT at the inbound buffer area')
#line_ManpowerofPacking= ax.plot(data['Iterations'], data['SMA_ManpowerofPacking'], label='AQT at the outbound buffer area')

plt.fill_between(data['Iterations'], 0, Cumulative_Time1, color=colors[0], alpha=0.75, label = "Workers for Forklift", linewidth=0, edgecolor='none')
plt.fill_between(data['Iterations'], Cumulative_Time1, Cumulative_Time2, color=colors[1], alpha=0.75, label = "Workers for Unpacking", linewidth=0, edgecolor='none')
plt.fill_between(data['Iterations'], Cumulative_Time2, Cumulative_Time3, color=colors[2], alpha=0.75, label = "Workers for Packing", linewidth=0, edgecolor='none')


# 设置图表标题和轴标签
plt.xlabel('Iterations', fontproperties={'family': 'serif','size': 10,'style': 'normal'},)
plt.ylabel('Quantity of Workers and Forklifts', fontproperties={'family': 'serif','size': 10,'style': 'normal'},)

# 添加虚线网格
plt.grid(True, linestyle='-.', alpha=0.3)

plt.xlim([0, max(data['Iterations'])])
plt.ylim([0, max(Cumulative_Time3)])

ax.xaxis.set_major_locator(MultipleLocator(2000))  # 每50个单位设置一个x轴主刻度
ax.yaxis.set_major_locator(MultipleLocator(100))  # 每10个单位设置一个y轴主刻度

plt.legend(prop={'family': 'serif','size': 10,'style': 'normal'}, frameon = False,bbox_to_anchor=(1, 0.5))
plt.tick_params(axis='both', labelsize=10)

# 定义字体属性字典
font_props = {'family': 'serif','size': 10,'style': 'normal'}
plt.legend(prop=font_props, frameon = False)

# 调整边距和布局
plt.subplots_adjust(left=0.10, right=0.95, top=0.95, bottom=0.15, wspace=0.01, hspace=0.01)

# 保存图像
plt.savefig('Optimization Process of Workers_5obj.pdf', dpi=800, pad_inches=0)

# 显示图形
plt.show()