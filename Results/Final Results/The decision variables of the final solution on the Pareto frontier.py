import pandas as pd
import matplotlib.pyplot as plt
import numpy as np

# 创建表格数据
# data = {
#     "d (days)": list(range(1, 32)),
#     r'$s_{R,t^{\mathrm{am}}_d}$': [2, 2, 2, 3, 4, 3, 4, 2, 2, 2, 3, 2, 2, 3, 2, 2, 6, 4, 2, 2, 2, 2, 3, 9, 4, 2, 7, 2, 3, 3, 6],
#     r'$s_{I,t^{\mathrm{am}}_d}$': [12, 4, 3, 3, 4, 2, 3, 2, 2, 2, 2, 4, 10, 12, 2, 8, 2, 3, 6, 2, 6, 2, 2, 3, 2, 4, 2, 3, 2, 2, 5],
#     r'$s_{O,t^{\mathrm{am}}_d}$': [3, 5, 9, 2, 2, 3, 3, 2, 2, 3, 2, 2, 2, 3, 6, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 2, 2, 2, 2, 5, 3],
#     r'$f_{R,t^{\mathrm{am}}_d}$': [2, 2, 2, 3, 4, 3, 4, 2, 2, 2, 3, 2, 2, 3, 2, 2, 6, 4, 2, 2, 2, 2, 3, 9, 4, 2, 7, 2, 3, 3, 6],
#     r'$f_{I,t^{\mathrm{am}}_d}$': [7, 4, 3, 3, 4, 2, 3, 2, 2, 2, 2, 4, 10, 12, 2, 8, 2, 3, 5, 2, 6, 2, 2, 3, 2, 4, 2, 3, 2, 2, 5],
#     r'$f_{O,t^{\mathrm{am}}_d}$': [3, 5, 9, 2, 2, 3, 3, 2, 2, 3, 2, 2, 2, 3, 5, 2, 2, 2, 2, 2, 2, 2, 2, 3, 3, 2, 2, 2, 2, 5, 3],
#     r'$s_{R,t^{\mathrm{pm}}_d}$': [3, 6, 2, 5, 2, 2, 6, 2, 7, 5, 2, 2, 2, 2, 6, 2, 3, 4, 2, 4, 2, 3, 6, 2, 6, 7, 3, 2, 9, 6, 3],
#     r'$s_{I,t^{\mathrm{pm}}_d}$': [8, 12, 11, 10, 17, 17, 14, 9, 20, 10, 7, 14, 9, 10, 11, 10, 9, 15, 11, 17, 12, 16, 16, 8, 12, 11, 9, 17, 11, 11, 9],
#     r'$s_{O,t^{\mathrm{pm}}_d}$': [4, 3, 5, 8, 2, 2, 3, 11, 2, 5, 12, 6, 3, 2, 2, 4, 6, 2, 4, 5, 6, 3, 2, 3, 2, 2, 5, 3, 2, 2, 7],
#     r'$f_{R,t^{\mathrm{pm}}_d}$': [3, 6, 2, 5, 2, 2, 6, 2, 7, 5, 2, 2, 2, 2, 6, 2, 3, 4, 2, 4, 2, 3, 6, 2, 6, 7, 3, 2, 9, 6, 3],
#     r'$f_{I,t^{\mathrm{pm}}_d}$': [8, 10, 7, 7, 10, 13, 10, 9, 9, 8, 7, 12, 9, 10, 11, 10, 9, 15, 11, 7, 10, 12, 10, 8, 9, 11, 9, 14, 11, 11, 9],
#     r'$f_{O,t^{\mathrm{pm}}_d}$': [4, 3, 5, 5, 2, 2, 3, 6, 2, 5, 8, 4, 3, 2, 2, 3, 6, 2, 4, 5, 6, 3, 2, 3, 2, 2, 4, 3, 2, 2, 5]
# }

data = {
    "d (days)": list(range(1, 32)),
    r'$s^{dr}_{R,t^{am}_d}$': [3, 3, 3, 5, 5, 4, 12, 13, 7, 3, 2, 2, 16, 2, 7, 5, 6, 3, 4, 2, 9, 4, 5, 14, 4, 3, 2, 14, 8, 13, 3],
    r'$s^{dr}_{I,t^{am}_d}$': [2, 11, 13, 2, 3, 7, 2, 2, 10, 4, 2, 2, 2, 2, 6, 9, 8, 4, 5, 4, 4, 6, 3, 3, 11, 10, 3, 2, 2, 2, 13],
    r'$s^{up}_{I,t^{am}_d}$': [3, 3, 2, 9, 7, 3, 14, 4, 2, 11, 3, 2, 15, 8, 2, 7, 5, 2, 10, 18, 9, 8, 5, 16, 4, 7, 5, 5, 8, 5, 10],
    r'$s^{dr}_{O,t^{am}_d}$': [11, 2, 3, 7, 7, 2, 3, 2, 2, 11, 14, 12, 2, 14, 2, 2, 2, 9, 6, 10, 2, 7, 9, 2, 2, 4, 13, 2, 2, 2, 2],
    r'$s^{pa}_{O,t^{am}_d}$': [27, 2, 2, 3, 26, 28, 27, 2, 27, 12, 12, 23, 28, 24, 28, 23, 4, 28, 5, 4, 15, 3, 9, 27, 27, 26, 23, 26, 2, 11, 28],
    r'$f_{R,t^{am}_d}$': [3, 3, 3, 5, 5, 4, 12, 13, 7, 3, 2, 2, 16, 2, 7, 5, 6, 3, 4, 2, 9, 4, 5, 14, 4, 3, 2, 14, 8, 13, 3],
    r'$f_{I,t^{am}_d}$': [2, 11, 13, 2, 3, 7, 2, 2, 10, 4, 2, 2, 2, 2, 6, 9, 8, 4, 5, 4, 4, 6, 3, 3, 11, 10, 3, 2, 2, 2, 13],
    r'$f_{O,t^{am}_d}$': [11, 2, 3, 7, 7, 2, 3, 2, 2, 11, 14, 12, 2, 14, 2, 2, 2, 9, 6, 10, 2, 7, 9, 2, 2, 4, 13, 2, 2, 2, 2],
    r'$s^{dr}_{R,t^{pm}_d}$': [6, 7, 8, 10, 6, 4, 6, 5, 8, 6, 5, 3, 2, 3, 8, 2, 6, 8, 4, 14, 3, 4, 7, 2, 6, 10, 4, 2, 10, 7, 5],
    r'$s^{dr}_{I,t^{pm}_d}$': [9, 10, 7, 8, 12, 9, 10, 9, 9, 10, 8, 13, 13, 9, 11, 7, 9, 8, 12, 4, 10, 10, 11, 8, 10, 8, 8, 15, 9, 12, 9],
    r'$s^{up}_{I,t^{pm}_d}$': [18, 19, 20, 12, 14, 16, 8, 17, 21, 9, 18, 19, 7, 12, 20, 9, 17, 20, 12, 3, 13, 14, 17, 6, 14, 13, 15, 17, 12, 16, 11],
    r'$s^{dr}_{O,t^{pm}_d}$': [5, 2, 4, 2, 2, 8, 3, 6, 3, 3, 7, 6, 3, 7, 2, 11, 3, 5, 5, 2, 7, 6, 2, 6, 3, 3, 7, 2, 2, 2, 5],
    r'$s^{pa}_{O,t^{pm}_d}$': [3, 28, 28, 27, 5, 2, 3, 28, 3, 18, 15, 5, 2, 6, 2, 6, 26, 2, 25, 26, 14, 25, 19, 4, 3, 3, 7, 3, 26, 19, 3],
    r'$f_{R,t^{pm}_d}$': [6, 7, 8, 10, 6, 4, 6, 5, 8, 6, 5, 3, 2, 3, 8, 2, 6, 8, 4, 14, 3, 4, 7, 2, 6, 10, 4, 2, 10, 7, 5],
    r'$f_{I,t^{pm}_d}$': [9, 10, 7, 8, 12, 9, 10, 9, 9, 10, 8, 13, 13, 9, 11, 7, 9, 8, 12, 4, 10, 10, 11, 8, 10, 8, 8, 15, 9, 12, 9],
    r'$f_{O,t^{pm}_d}$': [5, 2, 4, 2, 2, 8, 3, 6, 3, 3, 7, 6, 3, 7, 2, 11, 3, 5, 5, 2, 7, 6, 2, 6, 3, 3, 7, 2, 2, 2, 5]
}



df = pd.DataFrame(data)

# 删除 "d (days)" 列，设置为索引
df.set_index("d (days)", inplace=True)

# 设置字体
plt.rcParams.update({'font.size': 8, 'font.family': 'serif', 'font.serif': ['Arial', 'Helvetica', 'DejaVu Sans']})

# 生成热力图
plt.figure(figsize=(6, 6), dpi=1000)
plt.imshow(df, cmap="YlGnBu", aspect='auto')

# 添加颜色条
plt.colorbar()

# 添加 x 轴和 y 轴标签并设置字体
plt.ylabel('Working days', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'})
xticks = plt.xticks(np.arange(len(df.columns)), df.columns, ha='right', fontproperties={'family': 'serif', 'size': 6, 'style': 'normal'})
plt.yticks(np.arange(len(df.index)), df.index, fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'})

plt.legend(prop={'family': 'serif', 'size': 8, 'style': 'normal'}, frameon = False)

# 在每个单元格中添加数值
for i in range(len(df.index)):
    for j in range(len(df.columns)):
        plt.text(j, i, df.iloc[i, j], ha='center', va='center', color='black', fontproperties={'family': 'serif', 'size': 8, 'style': 'normal'})

# 获取当前x轴标签
labels = plt.gca().get_xticklabels()

# 设置标签位置
for label in labels:
    label.set_x(label.get_position()[0] + 0.1)  # 调整偏移量，这里设置为0.1，你可以根据需要调整

# 显示图形
plt.tight_layout()  # 可以加上这个调整布局，以防止x轴标签被剪切
# 调整边距和布局
plt.subplots_adjust(left=0.08, right=1.00, top=0.99, bottom=0.05, wspace=0.00, hspace=0.00)

# 保存图像
plt.savefig('The decision variables of the final solution on the Pareto frontier in Scenario II.pdf', dpi=800, pad_inches=0)
plt.show()