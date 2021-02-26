using System;

namespace Kufar
{
    public class DataParse
    {
        public int Id { get; set; }

        public int Idcategory { get; set; }

        public int IdParentCategory { get; set; }
        public string CategoryName { get; set; }

        public string Link { get; set; }


        public string Title { get; set; }
        public string Price { get; set; }
        public string UserName { get; set; }

        public DateTime updateDate { get; set; }

        /// <summary>Состояние (новое, б/у)</summary>
        public string condition { get; set; }

    }
}
