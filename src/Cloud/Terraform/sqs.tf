resource "aws_sqs_queue" "revival_queue" {
  name = "revival-queue${var.donate_craft_environment}"
}