resource "aws_lambda_function" "donatecraft" {
  function_name = "donatecraft-${var.donate_craft_version}"
  
  s3_bucket = "donatecraft"
  s3_key    = "donatecraft_api_${var.donate_craft_version}.zip"
  
  handler = "Web::Web.LambdaEntryPoint::FunctionHandlerAsync"
  runtime = "dotnet6"

  role = "${aws_iam_role.lambda_exec.arn}"
}

resource "aws_iam_role" "lambda_exec" {
  name = "serverless_example_lambda"

  assume_role_policy = <<EOF
  {
    "Version": "2012-10-17",
    "Statement": [
      {
        "Action": "sts:AssumeRole",
        "Principal": {
          "Service": "lambda.amazonaws.com"
        },
        "Effect": "Allow",
        "Sid": ""
      }
    ]
  }
  EOF
}