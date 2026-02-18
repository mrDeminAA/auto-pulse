# Скрипт для отправки команды на парсинг через RabbitMQ
# Требуется: curl

$RABBIT_MQ_URL = "http://localhost:15672"
$RABBIT_MQ_USER = "guest"
$RABBIT_MQ_PASS = "guest"
$QUEUE_NAME = "parse-cars-queue"

# URL для парсинга (страница автомобиля на Che168)
$PARSE_URL = "https://www.che168.com/home?infoid=56640428&pvareaid=108948&cpcid=0&isrecom=0&queryid=1771433708739$0$20AFCA6F-2E3D-4627-9D07-F3E7D87C367F$95577$1&cartype=60&offertype=10005&offertag=0&activitycartype=0&userareaid=0&adfromid=0&fromtag=0&ext=%7B%22urltype%22%3A%22%22%7D&otherstatisticsext=%7B%22abtest0923%22%3A%22%22%2C%22carrange%22%3A0%2C%22cartype%22%3A60%2C%22dealertype%22%3A0%2C%22eventid%22%3A%22usc_2sc_mc_mclby_cydj_click%22%2C%22history%22%3A%22%E5%88%97%E8%A1%A8%E9%A1%B5%22%2C%22is_remote%22%3A0%2C%22location0923%22%3A0%2C%22offertype%22%3A0%2C%22pvareaid%22%3A%220%22%2C%22srecom%22%3A%220%22%7D&ispc=1&type=new"

# Создаём сообщение команды
$message = @{
    url = $PARSE_URL
    sourceName = "Che168"
    country = "China"
} | ConvertTo-Json

Write-Host "=== Отправка команды на парсинг ===" -ForegroundColor Green
Write-Host "URL: $PARSE_URL"
Write-Host "Queue: $QUEUE_NAME"
Write-Host ""

# Отправляем через RabbitMQ Management API
$publishUrl = "$RABBIT_MQ_URL/api/exchanges/%2F/amq.default/publish"
$auth = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("$($RABBIT_MQ_USER):$($RABBIT_MQ_PASS)"))

$body = @{
    properties = @{
        content_type = "application/json"
        delivery_mode = 2
    }
    routing_key = $QUEUE_NAME
    payload = $message
    payload_encoding = "string"
} | ConvertTo-Json -Depth 10

$headers = @{
    "Authorization" = "Basic $auth"
    "Content-Type" = "application/json"
}

try {
    $response = Invoke-RestMethod -Uri $publishUrl -Method Post -Headers $headers -Body $body
    Write-Host "✓ Команда успешно отправлена!" -ForegroundColor Green
    Write-Host "Message Count: $($response.message_count)"
} catch {
    Write-Host "✗ Ошибка отправки: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Альтернатива: отправьте через RabbitMQ UI:" -ForegroundColor Yellow
    Write-Host "1. Откройте http://localhost:15672"
    Write-Host "2. Login: guest / Password: guest"
    Write-Host "3. Перейдите в 'Exchanges'"
    Write-Host "4. Выберите 'amq.default'"
    Write-Host "5. В разделе 'Publish message':"
    Write-Host "   - Routing key: $QUEUE_NAME"
    Write-Host "   - Payload: $message"
    Write-Host "   - Нажмите 'Publish message'"
}
