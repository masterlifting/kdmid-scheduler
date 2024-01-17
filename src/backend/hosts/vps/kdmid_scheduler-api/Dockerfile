FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env

WORKDIR /app

COPY . .

WORKDIR /app/hosts/vps/kdmid_scheduler-api

RUN dotnet restore

RUN dotnet publish -c Release -o publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app/hosts/vps/kdmid_scheduler-api/publish

# COPY ./hosts/vps/kdmid_scheduler-api/certs/Root-R5.crt /etc/ssl/certs/
# COPY ./hosts/vps/kdmid_scheduler-api/certs/Root-R5.crt /usr/local/share/ca-certific

# COPY ./hosts/vps/kdmid_scheduler-api/certs/alphasslcasha256g4.crt /etc/ssl/certs/
# COPY ./hosts/vps/kdmid_scheduler-api/certs/alphasslcasha256g4.crt /usr/local/share/ca-certific

COPY ./hosts/vps/kdmid_scheduler-api/certs/encoded/_.kdmid.ru.crt /etc/ssl/certs/
COPY ./hosts/vps/kdmid_scheduler-api/certs/encoded/_.kdmid.ru.crt /usr/local/share/ca-certificates/

COPY ./hosts/vps/kdmid_scheduler-api/certs/encoded/AlphaSSL_CA-SHA256-G4.crt /etc/ssl/certs/
COPY ./hosts/vps/kdmid_scheduler-api/certs/encoded/AlphaSSL_CA-SHA256-G4.crt /usr/local/share/ca-certificates/

COPY ./hosts/vps/kdmid_scheduler-api/certs/encoded/GlobalSign_Root_CA.crt /etc/ssl/certs/
COPY ./hosts/vps/kdmid_scheduler-api/certs/encoded/GlobalSign_Root_CA.crt /usr/local/share/ca-certificates/

# RUN chmod 644 /etc/ssl/certs/Root-R5.crt
# RUN chmod 644 /usr/local/share/ca-certificates/Root-R5.crt

# RUN chmod 644 /etc/ssl/certs/alphasslcasha256g4.crt
# RUN chmod 644 /usr/local/share/ca-certificates/alphasslcasha256g4.crt

RUN chmod 644 /etc/ssl/certs/_.kdmid.ru.crt
RUN chmod 644 /usr/local/share/ca-certificates/_.kdmid.ru.crt

RUN chmod 644 /etc/ssl/certs/AlphaSSL_CA-SHA256-G4.crt
RUN chmod 644 /usr/local/share/ca-certificates/AlphaSSL_CA-SHA256-G4.crt

RUN chmod 644 /etc/ssl/certs/GlobalSign_Root_CA.crt
RUN chmod 644 /usr/local/share/ca-certificates/GlobalSign_Root_CA.crt

RUN update-ca-certificates

# RUN sed -i 's/^MinProtocol\s*=.*$/MinProtocol = TLSv1/g' /etc/ssl/openssl.cnf && \
#     sed -i 's/^CipherString\s*=.*$/CipherString = DEFAULT@SECLEVEL=1/g' /etc/ssl/openssl.cnf

COPY --from=build-env /app/hosts/vps/kdmid_scheduler-api/publish .

ENTRYPOINT ["dotnet", "KdmidScheduler.Api.dll"]
