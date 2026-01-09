resource "aws_sqs_queue" "revival_queue" {
  name = "revival-queue${var.donate_craft_environment}"
  visibility_timeout_seconds = 300,
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.revival_dlq.arn
    maxReceiveCount     = 4
  })
}

resource "aws_sqs_queue" "revival_dlq" {
  name = "revival-dlq${var.donate_craft_environment}"
}