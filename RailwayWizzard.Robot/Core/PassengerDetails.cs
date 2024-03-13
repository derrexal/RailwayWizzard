namespace RailwayWizzard.Robot.Core;

public class PassengerDetails
{
    public string Surname { get; set; }
    public string Name { get; set; }
    public string Middlename { get; set; }
    public string Age { get; set; }
    public string DocumentNumber { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
        
    public PassengerDetails(string surname, string name, string middlename, string age, string documentNumber, string phoneNumber, string email)
    {
        Surname = surname;
        Name = name;
        Middlename = middlename;
        Age = age;
        DocumentNumber = documentNumber;
        PhoneNumber = phoneNumber;
        Email = email;
    }
}