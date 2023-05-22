global using Coravel;
global using Coravel.Invocable;

global using FluentEmail.Core;

global using InfluxDB.Client;
global using InfluxDB.Client.Api.Domain;
global using InfluxDB.Client.Writes;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

global using Newtonsoft.Json;

global using OpenAI.GPT3.Extensions;
global using OpenAI.GPT3.Interfaces;
global using OpenAI.GPT3.ObjectModels;

global using System.Collections.ObjectModel;
global using System.IO.Compression;
global using System.Net;
global using System.Net.Mail;
global using System.Text;

global using Telegram.Bot;
global using Telegram.Bot.Exceptions;
global using Telegram.Bot.Extensions.Polling;
global using Telegram.Bot.Types;
global using Telegram.Bot.Types.Enums;
global using Telegram.Bot.Types.ReplyMarkups;

global using VasilyKengele.Commands;
global using VasilyKengele.Entities;
global using VasilyKengele.Extensions;
global using VasilyKengele.Invocables;
global using VasilyKengele.LoggerAdapters;
global using VasilyKengele.Services;
