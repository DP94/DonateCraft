resource "aws_lambda_function" "donatecraft" {
  function_name = "donatecraft${var.donate_craft_version}"
  
  s3_bucket = "donatecraft"
  s3_key    = "donatecraft_api${var.donate_craft_version}.zip"
  
  handler = "Web::Web.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnet6"

  role = "arn:aws:iam::043470831800:role/donatecraft-AspNetCoreFunctionRole-JZ3T6L7H5ZXQ"
  
  timeout = 30
  memory_size = 512

  environment {
    variables = {
      DonateCraft__DonateCraftUiUrl = var.donate_craft_ui
      DonateCraft__JustGivingApiKey = var.just_giving_api_key
      DonateCraft__JustGivingApiUrl = var.just_giving_api_url
      DonateCraft__PlayerTableName  = aws_dynamodb_table.player.name
      DonateCraft__LockTableName    = aws_dynamodb_table.lock.name
      DonateCraft__CharityTableName = aws_dynamodb_table.charity.name
    }
  }
}