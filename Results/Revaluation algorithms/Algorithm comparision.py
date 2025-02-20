import pandas as pd
import numpy as np
import matplotlib
import matplotlib.pyplot as plt
from matplotlib.ticker import MultipleLocator
from scipy.interpolate import interp1d

fig = plt.figure(dpi=300)
ax = fig.add_subplot(111, projection='3d')
# Step 1: Load data and calculate moving averages
def load_data_and_calculate_ma(filepath):
    df = pd.read_csv(filepath)
    n = 160  # 设置滑动窗口大小
    df['SMA_Avg_StackingTime'] = df['Avg_StackingTime'].rolling(window=n, min_periods=1).mean()
    df['SMA_Forklifts'] = df[' Forklifts'].rolling(window=n, min_periods=1).mean()
    df['SMA_Manpower'] = df[' Manpower'].rolling(window=n, min_periods=1).mean()
    return df[['Iterations', 'SMA_Avg_StackingTime', 'SMA_Manpower', 'SMA_Forklifts']]

# Step 2: Identify Pareto front points
def is_pareto_efficient(costs):
    is_efficient = np.ones(costs.shape[0], dtype=bool)
    for i, c in enumerate(costs):
        if is_efficient[i]:
            is_efficient[is_efficient] = np.any(costs[is_efficient] < c, axis=1)
            is_efficient[i] = True
    return is_efficient

def find_pareto_points(df):
    costs = df[['SMA_Manpower', 'SMA_Forklifts', 'SMA_Avg_StackingTime']].to_numpy()
    efficient = is_pareto_efficient(costs)
    return df[efficient]

# Step 3: Identify the first Pareto point iteration and plot data from there onwards
def plot_data_from_first_pareto_point(original_df, pareto_df, filepath):
    first_pareto_iteration = pareto_df.iloc[0]['Iterations']
    df_subsequent = original_df[original_df['Iterations'] >= first_pareto_iteration]
    
    #n=600
    n=2000
    x, y, z = df_subsequent['SMA_Manpower'].rolling(window=n, min_periods=1).mean(), df_subsequent['SMA_Forklifts'].rolling(window=n, min_periods=1).mean(), df_subsequent['SMA_Avg_StackingTime'].rolling(window=n, min_periods=1).mean()

    # Fit a polynomial to y and z as functions of x
    #y_fit = np.poly1d(np.polyfit(x, y, 8))(np.unique(x))
    #z_fit = np.poly1d(np.polyfit(x, z, 8))(np.unique(x))
    y_fit = np.poly1d(np.polyfit(x, y, 6))(np.unique(x))
    z_fit = np.poly1d(np.polyfit(x, z, 6))(np.unique(x))

    # Plot the original data as points
    #ax.scatter(x, y, z, color='blue', s=5, alpha=1.0, marker='.', label="Approximated Pareto points")
    
    # Plot the fitted curves
    ax.plot(np.unique(x), y_fit, z_fit, color=[5/256,80/256,91/256], linewidth=2, label="MGD-SPSA" )
    
    # Mark the start and end points of the curve
    #start_x, end_x = np.unique(x)[0], np.unique(x)[-1]
    #start_y, end_y = y_fit[0], y_fit[-1]
    #start_z, end_z = z_fit[0], z_fit[-1]
    #ax.scatter([start_x, end_x], [start_y, end_y], [start_z, end_z], color='black', s=20, facecolors='none', marker='*', alpha=1.0)

    # Annotate the start and end points
    #ax.text(start_x, start_y, start_z+2, f'({35},{22},{start_z:.2f})', fontsize=5, color='black',horizontalalignment='center', verticalalignment='bottom')
    #ax.text(end_x-2, end_y-2, end_z+4, f'({int(round(end_x))},{int(round(end_y))},{end_z:.2f})', fontsize=5, color='black',horizontalalignment='center', verticalalignment='bottom')

# Step 4: Identify the first Pareto point iteration and plot data from there onwards
def plot_revolution_algorithm(times,la, filepath):
    df = pd.read_csv(filepath)
    
    #n=600
    x, y, z = df['Objective 2'], df['Objective 3'], df['Objective 1']

    # Fit a polynomial to y and z as functions of x
    y_fit = np.poly1d(np.polyfit(x, y, times))(np.unique(x))
    z_fit = np.poly1d(np.polyfit(x, z, times))(np.unique(x))
    
    # Plot the fitted curves
    mask = z < 1500
    ax.scatter(x[mask], y[mask], z[mask], s=1, marker='*', alpha=0.1)
    ax.plot(np.unique(x), y_fit, z_fit, linewidth=2, label=la)

# Example Usage:
filepath = 'Optimization_Process_5obj.csv'
data = load_data_and_calculate_ma(filepath)
pareto_points = find_pareto_points(data)
plot_data_from_first_pareto_point(data, pareto_points, filepath)
plot_revolution_algorithm(10, "NSGA2",'Approximated Pareto Frontier_NSGA2_5obj.csv')
plot_revolution_algorithm(10, "NSGA3",'Approximated Pareto Frontier_NSGA3_5obj.csv')
plot_revolution_algorithm(10, "MOEA/D",'Approximated Pareto Frontier_MOEAD_5obj.csv')
plot_revolution_algorithm(10, "SPEA2",'Approximated Pareto Frontier_SPEA2_5obj.csv')

ax.set_zlim(0, 1500)

ax.set_xlabel("Quantity of workers",labelpad=-12,fontdict = {'family': 'serif','size': 6,'style': 'normal'})
ax.set_ylabel("Quantity of forklifts", labelpad=-12,fontdict = {'family': 'serif','size': 6,'style': 'normal'})
ax.set_zlabel("AQT (min)", labelpad=-12,fontdict = {'family': 'serif','size': 6,'style': 'normal'})
ax.tick_params(axis='both', which='major', labelsize= 5, pad=-5)
plt.rcParams["font.family"] = "Times New Roman"
plt.tight_layout()
    
    
# 定义字体属性字典
font_props = {'family': 'serif','size': 6,'style': 'normal'}
plt.legend(prop=font_props, frameon=False, loc='upper left', bbox_to_anchor=(0.3, 0.6))
ax.xaxis.set_major_locator(MultipleLocator(100))  # 每50个单位设置一个x轴主刻度
    
ax.view_init(elev=8, azim=330)
plt.savefig('Algorithm comparision_5obj.pdf', bbox_inches=matplotlib.transforms.Bbox([[1.3, 0.5], [5, 3.3]]))
plt.show()