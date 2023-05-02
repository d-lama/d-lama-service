using d_lama_service.Repositories;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;

namespace d_lama_service
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private IWebHostEnvironment _environment;

        /// <summary>
        /// Constructor of the startup class. 
        /// </summary>
        /// <param name="configuration"> The configuration (appsettings.json). </param>
        /// <param name="env"> The environment (provided from env variables). </param>
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _environment = env;
        }

        /// <summary>
        /// Adds services to the program.
        /// </summary>
        /// <param name="services"> The services to configure. </param>
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "D-LAMA Service", Version = "v1" });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                 {
                     {
                           new OpenApiSecurityScheme
                             {
                                 Reference = new OpenApiReference
                                 {
                                     Type = ReferenceType.SecurityScheme,
                                     Id = "Bearer"
                                 }
                             },
                             new string[] {}
                     }
                 });

            });

            services.AddTransient<IUnitOfWork, UnitOfWork>(); // DI

            var connectionString = Configuration.GetConnectionString("prd");
            if (_environment.IsDevelopment())
            {
                connectionString = _environment.GetEnvironmentVariable("ASPNETCORE_TST");
            }

            services.AddDbContext<DataContext>(
                options =>
                {
                    options.UseSqlServer(connectionString);
                });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Defining standards
                    ValidIssuer = Configuration["Jwt:Issuer"],
                    ValidAudience = Configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero,

                    // validating
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true
                };
            });

            services.AddAuthorization();

        }

        /// <summary>
        /// Configures the HTTP pipeline.
        /// </summary>
        /// <param name="app"> The builded application (API). </param>
        public void Configure(WebApplication app)
        {

            if (_environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            
            app.UseSwagger();
                app.UseSwaggerUI(options => {
                    options.OAuthUsePkce();
            });
            
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }
    }

}
