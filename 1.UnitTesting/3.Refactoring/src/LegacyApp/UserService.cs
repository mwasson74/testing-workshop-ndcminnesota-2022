namespace LegacyApp
{
  using System;

  public class UserService
  {
    private readonly IClientRepository _clientRepo;
    private readonly IUserCreditService _serviceClient;

    public UserService() { }

    public UserService(IClientRepository clientRepo, IUserCreditService serviceClient)
    {
      _clientRepo = clientRepo;
      _serviceClient = serviceClient;
    }

    public bool AddUser(string firname, string surname, string email, DateTime dateOfBirth, int clientId)
    {
      if (string.IsNullOrEmpty(firname) || string.IsNullOrEmpty(surname))
      {
        return false;
      }

      if (!email.Contains("@") && !email.Contains("."))
      {
        return false;
      }

      var now = DateTime.Now;
      int age = now.Year - dateOfBirth.Year;
      if (now.Month < dateOfBirth.Month || (now.Month == dateOfBirth.Month && now.Day < dateOfBirth.Day)) age--;

      if (age < 21)
      {
        return false;
      }

      var client = _clientRepo.GetById(clientId);
      var user = new User
      {
        Client = client,
        DateOfBirth = dateOfBirth,
        EmailAddress = email,
        Firstname = firname,
        Surname = surname
      };

      if (client.Name == "VeryImportantClient")
      {
        // Skip credit check
        user.HasCreditLimit = false;
      }
      else if (client.Name == "ImportantClient")
      {
        // Do credit check and double credit limit
        user.HasCreditLimit = true;
        var creditLimit = _serviceClient.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
        creditLimit *= 2;
        user.CreditLimit = creditLimit;
      }
      else
      {
        // Do credit check
        user.HasCreditLimit = true;
        using (var userCreditService = new UserCreditServiceClient())
        {
          var creditLimit = userCreditService.GetCreditLimit(user.Firstname, user.Surname, user.DateOfBirth);
          user.CreditLimit = creditLimit;
        }
      }

      if (user.HasCreditLimit && user.CreditLimit < 500)
      {
        return false;
      }

      UserDataAccess.AddUser(user);
      return true;
    }
  }
}
