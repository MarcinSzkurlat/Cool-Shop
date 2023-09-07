using System.Linq;
using Codecool.CodecoolShop.Data;
using Codecool.CodecoolShop.Models.UserData;
using Codecool.CodecoolShop.Models.ViewModels;

namespace Codecool.CodecoolShop.Services;

public class AddressService
{
    private readonly CodeCoolShopDBContext _dbContext;

    public AddressService(CodeCoolShopDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(object address)
    {
        switch (address)
        {
            case BillingAddressModel billingAddressModel:
                DeleteExistingBillingAddress(billingAddressModel.UserId);
                _dbContext.BillingAddressModels.Add(billingAddressModel);
                break;
            case ShippingAddressModel shippingAddressModel:
                DeleteExistingShippingAddress(shippingAddressModel.UserId);
                _dbContext.ShippingAddressModels.Add(shippingAddressModel);
                break;
        }

        Save();
    }

    public void DeleteExistingBillingAddress(string userId)
    {
        var addressToDelete = _dbContext.BillingAddressModels
            .Where(x => x.UserId == userId)
            .ToList();
        _dbContext.BillingAddressModels.RemoveRange(addressToDelete);
        Save();
    } 
    
    public void DeleteExistingShippingAddress(string userId)
    {
        var addressToDelete = _dbContext.ShippingAddressModels
            .Where(x => x.UserId == userId)
            .ToList();
        _dbContext.ShippingAddressModels.RemoveRange(addressToDelete);
        Save();
    }

    public void UpdateAddressWithUserId(FullBillingViewModel model, string userId)
    {
        model.BillingAddress.UserId = userId;
        model.ShippingAddress.UserId = userId;
    }

    public void Save()
    {
        _dbContext.SaveChanges();
    }

    public BillingAddressModel FindBillingAddress(string userId)
    {
        var bill = _dbContext.BillingAddressModels.FirstOrDefault(x=>x.UserId == userId);
        return bill;
    }    
    
    public ShippingAddressModel FindShippingAddress(string userId)
    {
        var ship = _dbContext.ShippingAddressModels.FirstOrDefault(x => x.UserId == userId);
        return ship;
    }

}