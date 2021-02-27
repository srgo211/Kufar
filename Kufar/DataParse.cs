using System;
using System.Collections.Generic;

namespace Kufar
{
    public class DataParse
    {
        #region Данные при первом запросе
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
        #endregion

        #region Карточка товара
        /// <summary>Фото товара</summary>
        public string[] images { get; set; }
        /// <summary>Данные о товаре</summary>
        public Dictionary<string, string> oldAdParameters { get; set; }

        /// <summary>Описание товара</summary>
        public string descriptions { get; set; }

        /// <summary>Номер телефона</summary>
        public string phoneNomer { get; set; }
        #endregion

    }
}
