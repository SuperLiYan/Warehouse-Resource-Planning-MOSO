using O2DESNet.Standard;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Warehouse_Sim_opti.Simulation_Entities;
using Warehouse_Sim_opti.Simulation_Modules;

namespace Warehouse_Sim_opti
{
    internal class Forklift
    {
        #region Statics
        public double MaxCapacity { get; set; }
        #endregion

        #region Dynamics        
        public bool Status = true;
        public bool IfFull = false;
        public bool OnLoading = false;
        public double ExisitingLoad { get; set; }
        public List<Box> LoadedBoxes = new List<Box>();
        public List<Batch> loadedBatches = new List<Batch>();
        public DateTime ExpectedFinishingLoadingTime = new DateTime();
        #endregion

        public void LoadBoxes(List<Box> boxes)
        {
            double weightofconsolidatedBoxes = boxes.Sum(box => box.Weight);
            if (ExisitingLoad + weightofconsolidatedBoxes <= MaxCapacity)
            {
                LoadedBoxes = boxes.Concat(LoadedBoxes).ToList<Box>();
                ExisitingLoad += weightofconsolidatedBoxes;
            }
        }

        public void LoadBoxes(Box box)
        {
            if (ExisitingLoad + box.Weight <= MaxCapacity)
            {
                LoadedBoxes.Add(box);
                ExisitingLoad += box.Weight;
            }
        }

        public void LoadBatches(Batch batch)
        {
            double weight = batch.Weight;

            if (ExisitingLoad + weight <= MaxCapacity)
            {
                loadedBatches.Add(batch);
                ExisitingLoad += weight;
            }
        }

        public void LoadBatches(List<Batch> batches)
        {
            double weightofconsolidatedBoxes = batches.Sum(batch=>batch.Weight);

            if (ExisitingLoad + weightofconsolidatedBoxes <= MaxCapacity)
            {
                loadedBatches = batches.Concat(loadedBatches).ToList<Batch>();
                ExisitingLoad += weightofconsolidatedBoxes;
            }
        }

        public void ReleaseBoxes()
        {
            LoadedBoxes.Clear();
            ExisitingLoad = 0;
            ExpectedFinishingLoadingTime = DateTime.MinValue;
            IfFull = false;
            OnLoading = false;
        }

        public void ReleaseBatches()
        {
            loadedBatches.Clear();
            ExisitingLoad = 0;
            ExpectedFinishingLoadingTime = DateTime.MinValue;
            IfFull = false;
            OnLoading = false;
        }

        public void PutDownBoxes(int startPoint, int endPoint)
        {
            LoadedBoxes.RemoveRange(startPoint, endPoint);
        }

        public void BeIdle() { Status = true; }
        public void BeBusy() { Status = false; }
        public void BeFull() { IfFull = true; }

        public void BeOnLoading() { OnLoading = true;}
        public void BeNotOnLoading() { OnLoading = false; }

        public void UpdateExpectedFinishingLoadingTime(DateTime expectedtime) 
        {
            if (expectedtime >= ExpectedFinishingLoadingTime)
            {
                ExpectedFinishingLoadingTime = expectedtime;
            }
        }

        public Forklift(double maxcapacity)
        {
            MaxCapacity = maxcapacity;
        }
    }
}