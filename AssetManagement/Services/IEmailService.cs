﻿namespace AssetManagement.Services;

public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string content);
}