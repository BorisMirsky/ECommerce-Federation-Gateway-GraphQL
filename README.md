# ECommerce-Federation-Gateway-GraphQL
Отдельные сервисы и Gateway-сервис (Cart), который их объединяет через Apollo Federation.



Примерное описание:

3 Web API, каждое на своем порту (5001, 5002, 5003).

Gateway на порту 5000 — единственный, кто принимает внешние запросы.

Banana Cake Pop — "замена Swagger".

Всё поднимается либо через dotnet run в 4-х терминалах, либо через docker-compose.

