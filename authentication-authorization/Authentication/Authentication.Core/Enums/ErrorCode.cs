namespace Authentication.Core.Enums
{
    public enum ErrorCode
    {
        // Account
        UserAlreadyExists = 100,
        UsernamePasswordNotValid = 101,
        AccountLocked = 102,
        AccountDisabled = 103,
        AccountNotFound = 104,
        
        // Credentials
        NewPasswordEqualsOldPassword = 200,
        IncorrectPassword = 201,
        InvalidPassword = 202,
        InvalidEmail = 203
    }
}