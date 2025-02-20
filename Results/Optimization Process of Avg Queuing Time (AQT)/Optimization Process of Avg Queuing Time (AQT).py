import matplotlib.pyplot as plt
import pandas as pd
from matplotlib.ticker import MultipleLocator

# 创建高分辨率的图形对象
fig, ax = plt.subplots(figsize=(6, 3), dpi=800)

# 读取数据
file_path = 'Optimization_Process_5obj.csv'
data = pd.read_csv(file_path)


# 设置滑动窗口大小
n =3000  # 可以根据需要调整这个值
colors=[[84/256,134/256,135/256], [71/256,51/256,53/256], [5/256,80/256,91/256]]

# 计算滑动平均
data['SMA_Avg_StackingTime'] = data['Avg_StackingTime'].rolling(window=n, min_periods=1).mean()
data['SMA_Avg_StackingTime_At_Gate'] = data[' Avg_StackingTime_At_Gate'].rolling(window=n, min_periods=1).mean()
data['SMA_Avg_StackingTime_At_Inbound'] = data['Avg_StackingTime_At_Inbound'].rolling(window=n, min_periods=1).mean()
data['SMA_Avg_StackingTime_At_Outbound'] = data[' Avg_StackingTime_At_Outbound'].rolling(window=n, min_periods=1).mean()

# 绘制滑动平均曲线
line_Avg_StackingTime= ax.plot(data['Iterations'], data['SMA_Avg_StackingTime'], c="red", alpha=0.75, linewidth = 1.2, label='Total AQT')
#line_Avg_StackingTime_At_Gate= ax.plot(data['Iterations'], data['SMA_Avg_StackingTime_At_Gate'], label='AQT at the receiving dock')
#line_Avg_Avg_StackingTime_At_Inbound= ax.plot(data['Iterations'], data['SMA_Avg_StackingTime_At_Inbound'], label='AQT at the inbound buffer area')
#line_Avg_Avg_StackingTime_At_Outbound= ax.plot(data['Iterations'], data['SMA_Avg_StackingTime_At_Outbound'], label='AQT at the outbound buffer area')

Cumulative_Time1 = data['SMA_Avg_StackingTime_At_Gate']
Cumulative_Time2 = data['SMA_Avg_StackingTime_At_Gate'] + data['SMA_Avg_StackingTime_At_Inbound']
Cumulative_Time3 = data['SMA_Avg_StackingTime_At_Gate'] + data['SMA_Avg_StackingTime_At_Inbound'] + data['SMA_Avg_StackingTime_At_Outbound']

plt.fill_between(data['Iterations'], Cumulative_Time2, Cumulative_Time3, color=colors[2], alpha=0.75, label = "AQT in the outbound buffer area", linewidth=0, edgecolor='none')
plt.fill_between(data['Iterations'], Cumulative_Time1, Cumulative_Time2, color=colors[1], alpha=0.75, label = "AQT in the inbound buffer area", linewidth=0, edgecolor='none')
plt.fill_between(data['Iterations'], 0, Cumulative_Time1, color=colors[0], alpha=0.75, label = "AQT in the receiving dock", linewidth=0, edgecolor='none')

# 设置图表标题和轴标签
plt.xlabel('Iterations', fontproperties={'family': 'serif','size': 10,'style': 'normal'},)
plt.ylabel('AQT (min)', fontproperties={'family': 'serif','size': 10,'style': 'normal'},)

# 添加虚线网格
plt.grid(True, linestyle='-.', alpha=0.3)

plt.xlim([0, max(data['Iterations'])])
plt.ylim([0, 700])

ax.xaxis.set_major_locator(MultipleLocator(2000))  # 每50个单位设置一个x轴主刻度
ax.yaxis.set_major_locator(MultipleLocator(100))  # 每10个单位设置一个y轴主刻度

# 定义字体属性字典
font_props = {'family': 'serif','size': 10,'style': 'normal'}
plt.legend(prop=font_props, frameon = False)


# 调整边距和布局
plt.subplots_adjust(left=0.10, right=0.95, top=0.95, bottom=0.15, wspace=0.01, hspace=0.01)

# 保存图像
plt.savefig('Optimization Process of Avg Queuing Time (AQT)_5obj.pdf', dpi=800, pad_inches=0)

# 显示图形
plt.show()