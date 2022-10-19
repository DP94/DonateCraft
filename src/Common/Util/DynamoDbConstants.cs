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
    public const string DeathReasonColName = "reason";
    public const string DeathPlayerNameColName = "playerName";
    public const string DeathCreatedDateColName = "createdDate";
    #endregion

    #region Lock
    public const string LockTableName = "Lock";
    public const string LockIdColName = "id";
    public const string LockUnlockedColName = "unlocked";
    public const string LockKeyColName = "key";
    #endregion
}