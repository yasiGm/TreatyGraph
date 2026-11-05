terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.0"
    }
  }
}

provider "azurerm" {
  features {}
  subscription_id = "4fc1cec7-0bdb-449e-86d0-998d8d48c774"
}

resource "azurerm_resource_group" "main" {
  name     = "rg-treatygraph"
  location = "uksouth"
}
resource "azurerm_container_app_environment" "main" {
  name                = "cae-treatygraph"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workload_profile {
    name                  = "Consumption"
    workload_profile_type = "Consumption"
  }
}

resource "azurerm_container_app" "api" {
  name                         = "treatygraph-api"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = azurerm_resource_group.main.name
  revision_mode                = "Single"
  workload_profile_name        = "Consumption"

  template {
    min_replicas = 0
    max_replicas = 1

    container {
      name   = "api"
      image  = "mcr.microsoft.com/k8se/quickstart:latest"
      cpu    = 0.25
      memory = "0.5Gi"
    }
  }

  ingress {
    external_enabled = true
    target_port      = 80

    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}

output "api_url" {
  value = "https://${azurerm_container_app.api.ingress[0].fqdn}"
}
resource "azurerm_mssql_server" "main" {
  name                         = "sql-treatygraph-yasi"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = "francecentral"
  version                      = "12.0"
  administrator_login          = "tgadmin"
  administrator_login_password = var.sql_admin_password
  minimum_tls_version          = "1.2"
}

resource "azurerm_mssql_firewall_rule" "my_ip" {
  name             = "my-mac"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = var.my_ip
  end_ip_address   = var.my_ip
}

resource "azurerm_mssql_firewall_rule" "azure_services" {
  name             = "allow-azure-services"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
resource "azapi_resource" "db" {
  type      = "Microsoft.Sql/servers/databases@2023-08-01-preview"
  name      = "treatygraph"
  parent_id = azurerm_mssql_server.main.id
  location  = azurerm_mssql_server.main.location

  body = {
    sku = {
      name     = "GP_S_Gen5_2"
      tier     = "GeneralPurpose"
      family   = "Gen5"
      capacity = 2
    }
    properties = {
      useFreeLimit                = true
      freeLimitExhaustionBehavior = "AutoPause"
      autoPauseDelay              = 60
      minCapacity                 = 0.5
      maxSizeBytes                = 34359738368
    }
  }
}