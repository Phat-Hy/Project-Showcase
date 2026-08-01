output "resource_group_name" {
  value       = azurerm_resource_group.rg.name
  description = "The name of the Resource Group"
}

output "acr_login_server" {
  value       = azurerm_container_registry.acr.login_server
  description = "The URL of the Container Registry"
}

output "database_fqdn" {
  value       = azurerm_postgresql_flexible_server.db.fqdn
  description = "The Hostname of the PostgreSQL Server"
}

output "container_app_fqdn" {
  value       = azurerm_container_app.app.ingress[0].fqdn
  description = "The FQDN of the deployed Container App"
}
