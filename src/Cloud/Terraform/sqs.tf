resource "aws_sqs_queue" "revival_queue" {
  name = "${var.donate_craft_environment}revival-queue"
}