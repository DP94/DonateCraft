﻿using System.Globalization;
using System.Security.Cryptography;
using Amazon.DynamoDBv2.Model;
using Common.Models;
using Common.Util;

namespace Cloud.Util;

public static class DynamoDbUtility
{
    public static Dictionary<string, AttributeValue> GetAttributesFromPlayer(Player player)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.PlayerIdColName, new AttributeValue(player.Id));
        attributeValues.TryAdd(DynamoDbConstants.PlayerNameColName, new AttributeValue(player.Name));
        if (player.Deaths?.Count > 0)
        {
            var deathList = new AttributeValue
            {
                L = new List<AttributeValue>()
            };
            foreach (var death in player.Deaths)
            {
                death.PlayerId = player.Id;
                deathList.L.Add(new AttributeValue
                {
                    M = GetAttributesFromDeath(death)
                });
            }
            attributeValues.TryAdd(DynamoDbConstants.PlayerDeathsColName, deathList);
        }
        if (player.Donations?.Count > 0)
        {
            var donationList = new AttributeValue
            {
                L = new List<AttributeValue>()
            };
            foreach (var donation in player.Donations)
            {
                donationList.L.Add(new AttributeValue
                {
                    M = GetAttributesFromDonation(donation)
                });
            }
            attributeValues.TryAdd(DynamoDbConstants.PlayerDonationsColName, donationList);
        }
        return attributeValues;
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromDeath(Death death)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.DeathIdColName, new AttributeValue(death.Id));
        attributeValues.TryAdd(DynamoDbConstants.DeathPlayerIdColName, new AttributeValue(death.PlayerId));
        attributeValues.TryAdd(DynamoDbConstants.DeathReasonColName, new AttributeValue(death.Reason));
        attributeValues.TryAdd(DynamoDbConstants.DeathCreatedDateColName, new AttributeValue(death.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")));
        return attributeValues;
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromLock(Lock theLock)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.LockIdColName, new AttributeValue(theLock.Id));
        attributeValues.TryAdd(DynamoDbConstants.LockUnlockedColName, new AttributeValue {BOOL = theLock.Unlocked});
        if (theLock.DonationId != null)
        {
            attributeValues.TryAdd(DynamoDbConstants.LockDonationIdColName, new AttributeValue(theLock.DonationId));
        }
        return attributeValues; 
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromCharity(Charity charity)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.CharityIdColName, new AttributeValue(charity.Id));
        attributeValues.TryAdd(DynamoDbConstants.CharityNameColName, new AttributeValue(charity.Name));
        attributeValues.TryAdd(DynamoDbConstants.CharityDescriptionColName, new AttributeValue(charity.Description));
        attributeValues.TryAdd(DynamoDbConstants.CharityURLColName, new AttributeValue(charity.Url));
        return attributeValues;
    }
    
    public static Dictionary<string, AttributeValue> GetAttributesFromDonation(Donation donation)
    {
        var attributeValues = new Dictionary<string, AttributeValue>();
        attributeValues.TryAdd(DynamoDbConstants.DonationIdColName, new AttributeValue(donation.Id));
        attributeValues.TryAdd(DynamoDbConstants.DonationAmountColName, new AttributeValue { N =  donation.Amount.ToString(CultureInfo.InvariantCulture) });
        attributeValues.TryAdd(DynamoDbConstants.DonationCharityIdColName, new AttributeValue { N = donation.CharityId.ToString()});
        attributeValues.TryAdd(DynamoDbConstants.DonationCharityNameColName, new AttributeValue(donation.CharityName));
        attributeValues.TryAdd(DynamoDbConstants.DonationCreatedDateColName, new AttributeValue(donation.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")));
        attributeValues.TryAdd(DynamoDbConstants.DonationPaidForByColName, new AttributeValue(donation.PaidForId));
        return attributeValues;
    }

    public static Player GetPlayerFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var player = new Player();

        if (attributeValues.TryGetValue(DynamoDbConstants.PlayerIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.PlayerNameColName, out var name))
        {
            player.Id = id.S;
            player.Name = name.S;
        }

        if (attributeValues.TryGetValue(DynamoDbConstants.PlayerDeathsColName, out var deaths))
        {
            foreach (var death in deaths.L.Select(deathAttributes => GetDeathFromAttributes(deathAttributes.M)))
            {
                death.PlayerId = player.Id;
                player.Deaths.Add(death);
            }
        }
        
        if (attributeValues.TryGetValue(DynamoDbConstants.PlayerDonationsColName, out var donations))
        {
            foreach (var donation in donations.L.Select(deathAttributes => GetDonationFromAttributes(deathAttributes.M)))
            {
                player.Donations.Add(donation);
            }
        }
        


        return player;
    }
    
    public static Death GetDeathFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var death = new Death();

        if (attributeValues.TryGetValue(DynamoDbConstants.DeathIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.DeathReasonColName, out var reason) &&
            attributeValues.TryGetValue(DynamoDbConstants.DeathCreatedDateColName, out var createdDate))
        {
            death.Id = id.S;
            death.Reason = reason.S;
            death.CreatedDate = DateTime.ParseExact(createdDate.S, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
        }
            
        return death;
    }
    
    public static Lock GetLockFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var newLock = new Lock();

        if (attributeValues.TryGetValue(DynamoDbConstants.LockIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.LockUnlockedColName, out var unlocked))
        {
            newLock.Id = id.S;
            newLock.Unlocked = unlocked.BOOL;
            if (attributeValues.TryGetValue(DynamoDbConstants.LockDonationIdColName, out var donationId))
            {
                newLock.DonationId = donationId.S;
            }
        }
            
        return newLock;
    }
    
    public static Charity GetCharityFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var charity = new Charity();

        if (attributeValues.TryGetValue(DynamoDbConstants.CharityIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.CharityNameColName, out var name) &&
            attributeValues.TryGetValue(DynamoDbConstants.CharityDescriptionColName, out var description) &&
            attributeValues.TryGetValue(DynamoDbConstants.CharityURLColName, out var url))
        {
            charity.Id = id.S;
            charity.Description = description.S;
            charity.Name = name.S;
            charity.Url = url.S;
        }
            
        return charity;
    }
    
    public static Donation GetDonationFromAttributes(Dictionary<string, AttributeValue> attributeValues)
    {
        var donation = new Donation();

        if (attributeValues.TryGetValue(DynamoDbConstants.DonationIdColName, out var id) &&
            attributeValues.TryGetValue(DynamoDbConstants.DonationCharityNameColName, out var charityName) &&
            attributeValues.TryGetValue(DynamoDbConstants.DonationCharityIdColName, out var charityId) &&
            attributeValues.TryGetValue(DynamoDbConstants.DonationAmountColName, out var amount) &&
            attributeValues.TryGetValue(DynamoDbConstants.DonationCreatedDateColName, out var createdDate) &&
            attributeValues.TryGetValue(DynamoDbConstants.DonationPaidForByColName, out var paidForBy))
        {
            donation.Id = id.S;
            donation.CharityId = Convert.ToInt32(charityId.N);
            donation.CharityName = charityName.S;
            donation.Amount = Convert.ToDouble(amount.N);
            donation.CreatedDate = DateTime.ParseExact(createdDate.S, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            donation.PaidForId = paidForBy.S;
        }
            
        return donation;
    }
}