namespace SharjahConferenceAPI.Services.Interfaces;

using System;
using Data.Entities;

public interface ITokenService
{
    (string Token, DateTime Expiration) GenerateToken(User user);
}
