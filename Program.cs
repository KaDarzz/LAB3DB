using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Lab3.Data;
using Lab3.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;

namespace Lab3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Чтение строки подключения из appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("DBConnection");

            // Регистрация сервисов
            builder.Services.AddDbContext<HairdressingContext>(options =>
                options.UseSqlServer(connectionString));

            // Регистрация кэширования и сессий
            builder.Services.AddMemoryCache();
            builder.Services.AddScoped<CachedDataService>();

            // Регистрация сессий
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            app.UseSession();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/")
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string strResponse = "<HTML><HEAD><TITLE>Главная страница</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY>";
                    strResponse += "<BR><A href='/table'>Таблицы</A>";
                    strResponse += "<BR><A href='/info'>Информация</A>";
                    strResponse += "<BR><A href='/searchform1'>SearchForm1</A>";
                    strResponse += "<BR><A href='/searchform2'>SearchForm2</A>";
                    strResponse += "</BODY></HTML>";
                    await context.Response.WriteAsync(strResponse);
                    return;
                }
                await next.Invoke();
            });

            app.Map("/searchform1", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    var dbContext = context.RequestServices.GetService<HairdressingContext>();

                    if (context.Request.Method == "GET")
                    {
                        // Получение текущих значений из Cookies
                        var selectedClientId = context.Request.Cookies["ClientId"] ?? "";
                        var selectedEmployeeId = context.Request.Cookies["EmployeeId"] ?? "";
                        var selectedServiceId = context.Request.Cookies["ServiceId"] ?? "";

                        // Получение данных из базы
                        var clients = await dbContext.Clients.ToListAsync();
                        var employees = await dbContext.Employees.ToListAsync();
                        var services = await dbContext.Services.ToListAsync();

                        // Генерация HTML формы
                        var html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Search Form 1</title></head><body>";
                        html += "<h1>Search Records</h1>";
                        html += "<form method='post'>";

                        html += "<label for='ClientId'>Client:</label><br/>";
                        html += "<select id='ClientId' name='ClientId'>";
                        foreach (var client in clients)
                        {
                            var selected = client.Id.ToString() == selectedClientId ? "selected" : "";
                            html += $"<option value='{client.Id}' {selected}>{client.FullName}</option>";
                        }
                        html += "</select><br/><br/>";

                        html += "<label for='EmployeeId'>Employee:</label><br/>";
                        html += "<select id='EmployeeId' name='EmployeeId'>";
                        foreach (var employee in employees)
                        {
                            var selected = employee.Id.ToString() == selectedEmployeeId ? "selected" : "";
                            html += $"<option value='{employee.Id}' {selected}>{employee.FullName}</option>";
                        }
                        html += "</select><br/><br/>";

                        html += "<label for='ServiceId'>Service:</label><br/>";
                        html += "<select id='ServiceId' name='ServiceId'>";
                        foreach (var service in services)
                        {
                            var selected = service.Id.ToString() == selectedServiceId ? "selected" : "";
                            html += $"<option value='{service.Id}' {selected}>{service.Name}</option>";
                        }
                        html += "</select><br/><br/>";

                        html += "<button type='submit'>Search</button>";
                        html += "</form>";
                        html += "</body></html>";

                        await context.Response.WriteAsync(html);
                    }
                    else if (context.Request.Method == "POST")
                    {
                        // Чтение данных формы
                        var formData = await context.Request.ReadFormAsync();
                        var clientId = formData["ClientId"];
                        var employeeId = formData["EmployeeId"];
                        var serviceId = formData["ServiceId"];

                        // Сохранение значений в Cookies
                        context.Response.Cookies.Append("ClientId", clientId);
                        context.Response.Cookies.Append("EmployeeId", employeeId);
                        context.Response.Cookies.Append("ServiceId", serviceId);

                        // Построение запроса
                        var query = dbContext.PerformedServices
                            .Include(ps => ps.Client)
                            .Include(ps => ps.Employee)
                            .Include(ps => ps.Service)
                            .AsQueryable();

                        if (int.TryParse(clientId, out int clientIdValue))
                        {
                            query = query.Where(ps => ps.ClientId == clientIdValue);
                        }

                        if (int.TryParse(employeeId, out int employeeIdValue))
                        {
                            query = query.Where(ps => ps.EmployeeId == employeeIdValue);
                        }

                        if (int.TryParse(serviceId, out int serviceIdValue))
                        {
                            query = query.Where(ps => ps.ServiceId == serviceIdValue);
                        }

                        var results = await query.ToListAsync();

                        // Генерация результатов
                        var html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Search Results</title></head><body>";
                        html += "<h1>Search Results</h1>";

                        if (results.Count > 0)
                        {
                            html += "<table border='1' style='border-collapse:collapse'>";
                            html += "<tr><th>ID</th><th>Client</th><th>Employee</th><th>Service</th><th>Service Date</th><th>Cost</th></tr>";
                            foreach (var record in results)
                            {
                                html += "<tr>";
                                html += $"<td>{record.Id}</td>";
                                html += $"<td>{record.Client?.FullName}</td>";
                                html += $"<td>{record.Employee?.FullName}</td>";
                                html += $"<td>{record.Service?.Name}</td>";
                                html += $"<td>{record.ServiceDate}</td>";
                                html += $"<td>{record.Cost}</td>";
                                html += "</tr>";
                            }
                            html += "</table>";
                        }
                        else
                        {
                            html += "<p>No results found.</p>";
                        }

                        html += "<br/><a href='/searchform1'>Back to Search</a>";
                        html += "</body></html>";

                        await context.Response.WriteAsync(html);
                    }
                });
            });


            // Обработка пути "/searchform2" с использованием сессии и POI моделей
            app.Map("/searchform2", appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    var dbContext = context.RequestServices.GetService<HairdressingContext>();

                    if (context.Request.Method == "GET")
                    {
                        // Получение данных из сессии
                        var selectedClientId = context.Session.GetString("ClientId") ?? "";
                        var selectedEmployeeId = context.Session.GetString("EmployeeId") ?? "";
                        var selectedServiceId = context.Session.GetString("ServiceId") ?? "";

                        // Загрузка данных из базы
                        var clients = await dbContext.Clients.ToListAsync();
                        var employees = await dbContext.Employees.ToListAsync();
                        var services = await dbContext.Services.ToListAsync();

                        // Формирование HTML формы
                        var html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Search Form 2</title></head><body>";
                        html += "<h1>Search Records (Session)</h1>";
                        html += "<form method='post'>";

                        html += "<label for='ClientId'>Client:</label><br/>";
                        html += "<select id='ClientId' name='ClientId'>";
                        foreach (var client in clients)
                        {
                            var selected = client.Id.ToString() == selectedClientId ? "selected" : "";
                            html += $"<option value='{client.Id}' {selected}>{client.FullName}</option>";
                        }
                        html += "</select><br/><br/>";

                        html += "<label for='EmployeeId'>Employee:</label><br/>";
                        html += "<select id='EmployeeId' name='EmployeeId'>";
                        foreach (var employee in employees)
                        {
                            var selected = employee.Id.ToString() == selectedEmployeeId ? "selected" : "";
                            html += $"<option value='{employee.Id}' {selected}>{employee.FullName}</option>";
                        }
                        html += "</select><br/><br/>";

                        html += "<label for='ServiceId'>Service:</label><br/>";
                        html += "<select id='ServiceId' name='ServiceId'>";
                        foreach (var service in services)
                        {
                            var selected = service.Id.ToString() == selectedServiceId ? "selected" : "";
                            html += $"<option value='{service.Id}' {selected}>{service.Name}</option>";
                        }
                        html += "</select><br/><br/>";

                        html += "<button type='submit'>Search</button>";
                        html += "</form>";
                        html += "</body></html>";

                        await context.Response.WriteAsync(html);
                    }
                    else if (context.Request.Method == "POST")
                    {
                        // Чтение данных формы
                        var formData = await context.Request.ReadFormAsync();
                        var clientId = formData["ClientId"];
                        var employeeId = formData["EmployeeId"];
                        var serviceId = formData["ServiceId"];

                        // Сохранение данных в сессию
                        context.Session.SetString("ClientId", clientId);
                        context.Session.SetString("EmployeeId", employeeId);
                        context.Session.SetString("ServiceId", serviceId);

                        // Построение запроса
                        var query = dbContext.PerformedServices
                            .Include(ps => ps.Client)
                            .Include(ps => ps.Employee)
                            .Include(ps => ps.Service)
                            .AsQueryable();

                        if (int.TryParse(clientId, out int clientIdValue))
                        {
                            query = query.Where(ps => ps.ClientId == clientIdValue);
                        }

                        if (int.TryParse(employeeId, out int employeeIdValue))
                        {
                            query = query.Where(ps => ps.EmployeeId == employeeIdValue);
                        }

                        if (int.TryParse(serviceId, out int serviceIdValue))
                        {
                            query = query.Where(ps => ps.ServiceId == serviceIdValue);
                        }

                        var results = await query.ToListAsync();

                        // Формирование HTML для отображения результатов
                        var html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Search Results</title></head><body>";
                        html += "<h1>Search Results</h1>";

                        if (results.Count > 0)
                        {
                            html += "<table border='1' style='border-collapse:collapse'>";
                            html += "<tr><th>ID</th><th>Client</th><th>Employee</th><th>Service</th><th>Service Date</th><th>Cost</th></tr>";
                            foreach (var record in results)
                            {
                                html += "<tr>";
                                html += $"<td>{record.Id}</td>";
                                html += $"<td>{record.Client?.FullName}</td>";
                                html += $"<td>{record.Employee?.FullName}</td>";
                                html += $"<td>{record.Service?.Name}</td>";
                                html += $"<td>{record.ServiceDate}</td>";
                                html += $"<td>{record.Cost}</td>";
                                html += "</tr>";
                            }
                            html += "</table>";
                        }
                        else
                        {
                            html += "<p>No results found.</p>";
                        }

                        html += "<br/><a href='/searchform2'>Back to Search</a>";
                        html += "</body></html>";

                        await context.Response.WriteAsync(html);
                    }
                });
            });



            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/table")
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string strResponse = "<HTML><HEAD><TITLE>Таблицы</TITLE></HEAD>" +
                     "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                     "<BODY>";
                    strResponse += "<BR><A href='/table/Clients'>Clients</A>";
                    strResponse += "<BR><A href='/table/EmployeeSchedules'>EmployeeSchedules</A>";
                    strResponse += "<BR><A href='/table/PerformedServices'>PerformedServices</A>";
                    strResponse += "<BR><A href='/table/Reviews'>Reviews</A>";
                    strResponse += "<BR><A href='/table/Services'>Services</A>";
                    strResponse += "<BR><A href='/table/Employees'>Employees</A>";
                    strResponse += "<BR><A href='/table/ServiceTypes'>ServiceTypes</A>";
                    strResponse += "</BODY></HTML>";
                    await context.Response.WriteAsync(strResponse);
                    return;
                }
                await next.Invoke();
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/table", out var remainingPath) && remainingPath.HasValue && remainingPath.Value.StartsWith("/"))
                {
                    context.Response.ContentType = "text/html; charset=utf-8"; // Установка Content-Type
                    var tableName = remainingPath.Value.Substring(1); // Убираем начальный слэш

                    var cachedService = context.RequestServices.GetService<CachedDataService>();

                    if (tableName == "Clients")
                    {
                        var list = cachedService.GetClient();
                        await RenderTable(context, list);
                    }
                    else if (tableName == "EmployeeSchedules")
                    {
                        var list = cachedService.GetEmployeeSchedules();
                        await RenderTable(context, list);
                    }
                    else if (tableName == "PerformedServices")
                    {
                        var list = cachedService.GetPerformedServices();
                        await RenderTable(context, list);
                    }
                    else if (tableName == "Reviews")
                    {
                        var list = cachedService.GetReviews();
                        await RenderTable(context, list);
                    }
                    else if (tableName == "Services")
                    {
                        var list = cachedService.GetServices();
                        await RenderTable(context, list);
                    }
                    else if (tableName == "Employees")
                    {
                        var list = cachedService.GetEmployees();
                        await RenderTable(context, list);
                    }
                    else if (tableName == "ServiceTypes")
                    {
                        var list = cachedService.GetServiceTypes();
                        await RenderTable(context, list);
                    }
                    else
                    {
                        // Если таблица не найдена, возвращаем 404
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsync("Таблица не найдена");
                    }

                    return; // Завершаем обработку запроса
                }
                await next.Invoke();
            });

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/info")
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    string strResponse = "<HTML><HEAD><TITLE>Информация</TITLE></HEAD>" +
                    "<META http-equiv='Content-Type' content='text/html; charset=utf-8'/>" +
                    "<BODY><H1>Информация:</H1>";
                    strResponse += "<BR> Сервер: " + context.Request.Host;
                    strResponse += "<BR> Путь: " + context.Request.Path;
                    strResponse += "<BR> Протокол: " + context.Request.Protocol;
                    strResponse += "<BR><A href='/'>Главная</A></BODY></HTML>";
                    await context.Response.WriteAsync(strResponse);
                    return;
                }
                await next.Invoke();
            });

            async Task RenderTable<T>(HttpContext context, IEnumerable<T> data)
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                var html = "<table border='1' style='border-collapse:collapse'>";

                var type = typeof(T);

                // Генерация заголовков таблицы на основе свойств типа
                html += "<tr>";
                foreach (var prop in type.GetProperties())
                {
                    // Пропускаем свойства, которые являются объектами других классов или коллекциями
                    if (!IsSimpleType(prop.PropertyType))
                    {
                        continue;
                    }

                    html += $"<th>{prop.Name}</th>";
                }
                html += "</tr>";

                foreach (var item in data)
                {
                    html += "<tr>";
                    foreach (var prop in type.GetProperties())
                    {
                        if (!IsSimpleType(prop.PropertyType))
                        {
                            continue;
                        }

                        var value = prop.GetValue(item);

                        if (value is DateTime dateValue)
                        {
                            html += $"<td>{dateValue.ToString("dd.MM.yyyy")}</td>";
                        }
                        else
                        {
                            html += $"<td>{value}</td>";
                        }
                    }
                    html += "</tr>";
                }

                html += "</table>";
                await context.Response.WriteAsync(html);
            }

            bool IsSimpleType(Type type)
            {
                // Примитивные типы и типы, которые считаются простыми (string, DateTime и т.д.)
                return type.IsPrimitive ||
                       type.IsValueType ||
                       type == typeof(string) ||
                       type == typeof(DateTime) ||
                       type == typeof(decimal);
            }

            app.Run();
        }
    }
}