﻿namespace WinterWay.Models.Database
{
    public class CalendarValueModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
        public string Archived { get; set; }
        public int SortOrder { get; set; }

        public int CalendarId { get; set; }
        public CalendarModel Calendar { get; set; }
    }
}