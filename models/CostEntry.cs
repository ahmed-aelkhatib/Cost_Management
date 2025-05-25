﻿namespace ERPtask.models
{
    public class CostEntry
    {
        public int CostID { get; set; }
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
