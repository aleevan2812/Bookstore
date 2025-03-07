﻿using Microsoft.AspNetCore.Authentication;

namespace Identity.API.Services
{
    public interface ILoginService<T>
    {
        Task<bool> ValidateCredentials(T user, string password);

        Task<T> FindByUsername(string username);

        Task<string> SignIn(T user);

        Task SignInAsync(T user, AuthenticationProperties properties, string authenticationMethod = null);

        Task SignOut();
    }
}
