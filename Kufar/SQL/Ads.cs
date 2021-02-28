using SQLite;
using System;

namespace Kufar.SQL
{



    public class Ads
    {
        [PrimaryKey, AutoIncrement, Unique]
        public int id { get; set; }


        public int idAd { get; set; }



        public int idCategory { get; set; }

        public int idParentCategory { get; set; }
        public string categoryName { get; set; }

        [Unique]
        public string link { get; set; }


        public string title { get; set; }
        public string price { get; set; }
        public string userName { get; set; }

        public DateTime updateDate { get; set; }

        /// <summary>Состояние (новое, б/у)</summary>
        public string condition { get; set; }



        /// <summary>Фото товара</summary>
        public string images { get; set; }
        /// <summary>Хар-ки товара</summary>
        public string oldAdParameters { get; set; }

        /// <summary>Описание товара</summary>
        public string descriptions { get; set; }


        /// <summary>Номер телефона</summary>
        [MaxLength(30)]
        public string phoneNumber { get; set; }



        /// <summary>Дата добавления/обновления записи в БД</summary>
        public DateTime dateWrite { get; set; } = DateTime.Now;

        /// <summary>статус </summary>
        public Status status { get; set; } = Status.Null;

    }
}
