using MySqlConnector;

var connStr = "Server=localhost; DataBase=test10x; port=3306; User Id=root; password="; //строка подключения (указываем в ней параметры подключения)
// Параметры подключения: сервер, имя БД, порт (можно посмотреть в настройках БД), пользователь, пароль

// Если у нас нет БД, то параметр не указываем, но после conn.Open() сразу вызываем CreateSchema (т.е. создаем)
// Тогда потом при создании таблиц надо будет в обязательном порядке указывать БД

MySqlConnection conn = new MySqlConnection(connStr);
conn.Open(); //открываем наше соединение
MySqlCommand cmd=  conn.CreateCommand();  // создаем команду для работы
cmd.CommandText = "CREATE TABLE IF NOT EXISTS`department`(" + //добвляем в свойство CommandText строку с SQL запросом
    "id int not null auto_increment primary key,"+
    "name varchar(200) not null default 'рога и копыта',"+
    "location varchar(200) not null)";
cmd.ExecuteNonQuery(); // функция, которая исполнит текст и ничего не вернет в обратку
var data = new List<Department>();// список,откуда будем брать ин-ю для БД
data.Add(new Department() { Name="Департамент 1", Location = "Москва"});
data.Add(new Department() { Location = "Бобруйск" });
foreach (var department in data) // перебираем циклом все эл-ты в массиве
{
    var sqlCmd = $"INSERT INTO `department`({(department.Name!=null ? "Name, " : "")} location) " +
    $"VALUES({(department.Name != null ? "'"+department.Name+"', ": "")} '{department.Location}')";
    cmd.CommandText = sqlCmd; // добавляем 
    cmd.ExecuteNonQuery ();
}
try    // пробуем блоки try catch
{
    Console.WriteLine("Введите номер записи для удаления:");
    var delId = Console.ReadLine() ?? "0"; // получаем в переменную индекс строки для удаления
    var delCmd = conn.CreateCommand();
    //delCmd.CommandText = "DELETE  FROM `test10x`.`department` WHERE `id`=" + delId;// создаем комманду на удаление
    //delCmd.ExecuteNonQuery();
    // команду, которую мы создали использовать опасно, т.к. пользователь можнт ввести
    // sql инъекцию, в результате которой произойдут неожиданные изменения в таблице
    // Чтобы избежать этого, мы будем создавать параметризованный запрос
    delCmd.CommandText = "DELETE  FROM `test10x`.`department` WHERE `id`=@identity";// знак @ означает параметр,после него идет название
    MySqlParameter idParam = new MySqlParameter("@identity", delId);// первое - название, второе - откуда получаем
    delCmd.Parameters.Add(idParam);// добавляем параметр к команде в свойство Parametrs
    delCmd.ExecuteNonQuery();
}
catch ( Exception e)
{
    Console.WriteLine($"Что-то пошло не так \n {e.Message}") ;
}

var selectedCmd = conn.CreateCommand();
selectedCmd.CommandText = "SELECT * FROM `test10x`.`department`";// выбираем всю таблицу
var departmentList = new List<Department>();// создадим список для объектов из таблицы БД

using (var reader = selectedCmd.ExecuteReader())// используем метод ExecuteReader для чтения с возвратом
{
    if (reader.HasRows)// есть ли хотя бы одна строка в таблице
    {
        while (reader.Read())// читаем из таблицы построчно
        {
            departmentList.Add( // добавляем этот объект в список
                new Department() {// создаем объект класса Department
                Id = reader.GetInt32(0),// вызываем методы с нужным типом данных, в скобках указываем номнер столбца
                Name = reader.GetString(1),
                Location = reader.GetString(2),
            }); 
        }
    }
}
departmentList.ForEach((d) => { Console.WriteLine(d); });
    conn.Close();  // закрываем подключение после работы с БД


public class Department // записи дьля внесения в БД будут экземплярами класса (для удобства)
{
    public int Id { get; set; }
    public string? Name { get; set; } = null;
    public string? Location { get; set; } = null;
    public override string ToString()// переопределили метод для удобства
    {
        return $"{Id}: {Name} -> {Location}";
    }
}
