namespace Common.Util;

public static class DynamoDbConstants
{
    #region Player
    public const string PlayerTableName = "Player";
    public const string PlayerIdColName = "id";
    public const string PlayerNameColName = "name";
    public const string PlayerDeathsColName = "deaths";
    public const string PlayerDonationsColName = "donations";
    #endregion
    
    #region Death
    public const string DeathIdColName = "id";
    public const string DeathPlayerIdColName = "playerId";
    public const string DeathReasonColName = "reason";
    public const string DeathCreatedDateColName = "createdDate";
    #endregion

    #region Lock
    public const string LockTableName = "Lock";
    public const string LockIdColName = "id";
    public const string LockUnlockedColName = "unlocked";
    public const string LockDonationIdColName = "donationId";
    #endregion
    
    #region Charity
    public const string CharityTableName = "Charity";
    public const string CharityIdColName = "id";
    public const string CharityNameColName = "name";
    public const string CharityDescriptionColName = "description";
    public const string CharityURLColName = "url";
    #endregion
    
    #region Donation
    public const string DonationIdColName = "id";
    public const string DonationAmountColName = "amount";
    public const string DonationCharityIdColName = "charityId";
    public const string DonationCharityNameColName = "charityName";
    public const string DonationCreatedDateColName = "createdDate";
    public const string DonationPaidForByColName = "paidForById";
    public const string DonationPrivateColName = "private";
    #endregion
}