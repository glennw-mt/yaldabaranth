using LiteDB;

public class RegionRecord
{
}

public class SectorRecord
{
}

public class MapRecord
{
}

public class DatabaseManager
{
  LiteDatabase map_db;
  public DatabaseManager()
  {
    map_db = new LiteDatabase("./map.db");
  }
}
