namespace Common.Util;

public static class DynamoDbConstants
{
    #region Player
    public const string PlayerTableName = "Player";

    public const string PlayerIdColName = "id";
    public const string PlayerNameColName = "name";
    #endregion
    
    #region Death
    public const string DeathTableName = "Death";
    public const string DeathIdColName = "id";
    public const string DeathPlayerIdColName = "playerId";
    public const string DeathReasonColName = "name";
    #endregion
}