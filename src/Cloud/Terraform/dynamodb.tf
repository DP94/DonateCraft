resource "aws_dynamodb_table" "player" {
  name           = "Player${var.donate_craft_environment}"
  billing_mode   = "PROVISIONED"
  read_capacity  = 100
  write_capacity = 100
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "lock" {
  name           = "Lock${var.donate_craft_environment}"
  billing_mode   = "PROVISIONED"
  read_capacity  = 100
  write_capacity = 100
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }
}

resource "aws_dynamodb_table" "charity" {
  name           = "Charity${var.donate_craft_environment}"
  billing_mode   = "PROVISIONED"
  read_capacity  = 100
  write_capacity = 100
  hash_key       = "id"

  attribute {
    name = "id"
    type = "S"
  }
}