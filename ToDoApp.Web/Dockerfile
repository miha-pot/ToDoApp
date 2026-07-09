# 1. faza: Uporabimo .NET 10 SDK, da podpira tvoj Target Framework
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Ključni korak: Kopiramo celotno rešitev (vse mape), 
# da bo .NET znotraj kontejnerja videl ToDoApp.Shared in ToDoApp.SharedUI
COPY . .

# Zdaj ko so vsi odvisni projekti skopirani v kontejner, se premaknemo v spletno mapo
WORKDIR "/src/ToDoApp.Web"

# Poženemo restore (zdaj bo uspešno našel vse Shared projekte)
RUN dotnet restore "ToDoApp.Web.csproj"

# Zgradimo in objavimo aplikacijo v mapo /app/publish
RUN dotnet publish "ToDoApp.Web.csproj" -c Release -o /app/publish

# 2. faza: Serviranje preko lahkega Nginx strežnika
FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html

# Kopiramo zgrajene statične datoteke frontenda v Nginx mapo
COPY --from=build /app/publish/wwwroot .

# Kopiramo konfiguracijo (poskrbi, da je datoteka nginx.conf v mapi ToDoApp.Web)
COPY ToDoApp.Web/nginx.conf /etc/nginx/nginx.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]