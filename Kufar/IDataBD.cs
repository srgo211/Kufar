using Kufar.SQL;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace Kufar
{
    interface IDataBD
    {
        /// <summary>
        /// Создание БД
        /// </summary>
        /// <param name="pathBD"></param>
        void CreateBD(IZennoPosterProjectModel project, string pathBD);

        void AddBD(string pathBD, DataParse dataParse);

        /// <summary>
        /// Получаем объявлеие для парсинга телефона и обновляем статус
        /// </summary>
        /// <param name="pathBD"></param>
        /// <param name="dataParse"></param>
        /// <returns></returns>
        Ads GetAdAndUpdateCheckPhoneBD(string pathBD, DataParse dataParse);
        void UpdateBD(string pathBD, int idBD);

        void DeleteBD(string pathBD, int idBD);

    }






}
