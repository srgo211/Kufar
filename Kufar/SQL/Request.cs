using SQLite;
using System;
using System.Diagnostics;

namespace Kufar.SQL
{
    public class Request
    {

        public void CreateBD(string databasePath)
        {

            using (var db = new SQLiteConnection(databasePath))
            {
                db.CreateTable<Ads>();
            }

        }

        public int InsertBD(string databasePath, DataParse dataParse)
        {
            Ads ad = logDataAds(dataParse);

            int results = -1;
            var options = new SQLiteConnectionString(databasePath, false);
            using (var conn = new SQLiteConnection(options))
            {
                try
                {
                    results = conn.Insert(ad);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return -1;
                }

                return results;
            }


        }

        /// <summary>Получаем объявление из БД для парсинга телефона</summary>
        public Ads GetAdFromBDForParsePhone(string databasePath, DataParse dataParse)
        {


            Ads results = null;
            var options = new SQLiteConnectionString(databasePath, false);
            using (var conn = new SQLiteConnection(options))
            {
                try
                {
                    results = conn.Table<Ads>().Where(a => a.checkPhone == false).FirstOrDefault();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    return null;
                }

                //обновляем запись
                results.checkPhone = true;
                try
                {
                    conn.Update(results);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);

                }

                return results;


            }


        }


        public static Ads logDataAds(DataParse data)
        {


            const int symbol = 30; //кол-во символов (....)
            const string charSymbol = "."; //кол-во символов (....)
            string parametrs = string.Empty;

            if (data.oldAdParameters != null)
            {
                foreach (var el in data.oldAdParameters)
                {
                    int keyLength = el.Key.Length;

                    int countPresent = symbol - keyLength;
                    string symb = String.Empty;
                    for (int a = 0; a < countPresent; a++) symb += charSymbol;

                    parametrs += $"{el.Key}{symb}{el.Value}\n";
                }
            }


            string img = string.Empty;
            if (data.images != null)
            {
                img = String.Join("\n", data.images);
            }

            string log =
                 $"Id- {data.Id}\n" +
                 $"Link- {data.Link}\n" +
                $"Категория- {data.CategoryName}\n" +
                 $"Название- {data.Title}\n" +
                 $"Цена товара- {data.Price}\n" +
                 $"Состояние товара- {data.condition}\n" +
                 $"Хар-ки товара- \n{parametrs}\n" +
                 $"Описание товара- {data.descriptions}\n" +
                 $"ID категории- {data.Idcategory}\n" +
                 $"ID ПОДкатегории- {data.IdParentCategory}\n" +
                 $"Ссылки на фото - {img}\n" +
                 $"Номер телефона- {data.phoneNomer}\n" +
                 $"Дата публикации- {data.updateDate}\n" +
                 $"Имя пользователя- {data.UserName}\n";



            Ads ad = new Ads()
            {
                categoryName = data.CategoryName,

                condition = data.condition,
                updateDate = data.updateDate,

                descriptions = data.descriptions,
                idAd = data.Id,
                idCategory = data.Idcategory,
                idParentCategory = data.IdParentCategory,
                images = img,
                link = data.Link,
                oldAdParameters = parametrs,
                phoneNomer = data.phoneNomer,
                price = data.Price,
                title = data.Title,
                userName = data.UserName,

            };

            return ad;


        }
    }
}
