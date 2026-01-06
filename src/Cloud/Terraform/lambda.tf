resource "aws_lambda_function" "donatecraft" {
  function_name = "donatecraft${var.donate_craft_environment}"

  s3_bucket = "donatecraft"
  s3_key    = "donatecraft_api${var.donate_craft_environment}_${var.git_commit}.zip"

  handler = "Web::Web.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnet10"

  role = aws_iam_role.lambda_role.arn

  timeout     = 60
  memory_size = 512

  environment {
    variables = {
      DonateCraft__DonateCraftUiUrl = var.donate_craft_ui
      DonateCraft__JustGivingApiKey = var.just_giving_api_key
      DonateCraft__JustGivingApiUrl = var.just_giving_api_url
      DonateCraft__PlayerTableName  = aws_dynamodb_table.player.name
      DonateCraft__LockTableName    = aws_dynamodb_table.lock.name
      DonateCraft__CharityTableName = aws_dynamodb_table.charity.name
      DonateCraft__RevivalQueueUrl  = aws_sqs_queue.revival_queue.url
    }
  }
}

resource "aws_lambda_function" "revival_lambda" {
  function_name = "revival-lambda${var.donate_craft_environment}"

  s3_bucket = "donatecraft"
  s3_key    = "revival-lambda${var.donate_craft_environment}_${var.git_commit}.zip"

  handler = "RevivalLambda::RevivalLambda.Function::HandleRequest"

  runtime = "dotnet10"

  role = aws_iam_role.lambda_role.arn

  timeout     = 120
  memory_size = 512
}

resource "aws_lambda_event_source_mapping" "revival_lambda_mapping" {
  function_name = aws_lambda_function.revival_lambda.arn
  event_source_arn = aws_sqs_queue.revival_queue.arn
}