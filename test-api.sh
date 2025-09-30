#!/bin/bash

# Test script for WinterArc API
# This script tests the main API endpoints

API_URL="http://localhost:8080"
TOKEN=""

echo "=== WinterArc API Integration Tests ==="
echo ""

# Test 1: Register User
echo "Test 1: Register User"
REGISTER_RESPONSE=$(curl -s -X POST "$API_URL/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "email": "test@example.com",
    "password": "TestPassword123"
  }')

echo "Response: $REGISTER_RESPONSE"
TOKEN=$(echo $REGISTER_RESPONSE | grep -o '"token":"[^"]*' | sed 's/"token":"//')
echo "Token obtained: ${TOKEN:0:20}..."
echo ""

# Test 2: Login
echo "Test 2: Login"
LOGIN_RESPONSE=$(curl -s -X POST "$API_URL/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "testuser",
    "password": "TestPassword123"
  }')

echo "Response: $LOGIN_RESPONSE"
echo ""

# Test 3: Create Goal
echo "Test 3: Create Goal"
GOAL_RESPONSE=$(curl -s -X POST "$API_URL/goals" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Exercise Daily",
    "description": "Go for a 30-minute run",
    "xpReward": 150
  }')

echo "Response: $GOAL_RESPONSE"
GOAL_ID=$(echo $GOAL_RESPONSE | grep -o '"id":[0-9]*' | sed 's/"id"://')
echo "Goal ID: $GOAL_ID"
echo ""

# Test 4: Get All Goals
echo "Test 4: Get All Goals"
GOALS_RESPONSE=$(curl -s -X GET "$API_URL/goals" \
  -H "Authorization: Bearer $TOKEN")

echo "Response: $GOALS_RESPONSE"
echo ""

# Test 5: Create Check-In
echo "Test 5: Create Check-In"
CHECKIN_RESPONSE=$(curl -s -X POST "$API_URL/goals/$GOAL_ID/checkins" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "note": "Completed 5k run today!"
  }')

echo "Response: $CHECKIN_RESPONSE"
echo ""

# Test 6: Get User Overview
echo "Test 6: Get User Overview"
OVERVIEW_RESPONSE=$(curl -s -X GET "$API_URL/me/overview" \
  -H "Authorization: Bearer $TOKEN")

echo "Response: $OVERVIEW_RESPONSE"
echo ""

# Test 7: Update Goal
echo "Test 7: Update Goal"
UPDATE_RESPONSE=$(curl -s -X PUT "$API_URL/goals/$GOAL_ID" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "title": "Exercise Daily - Updated",
    "isCompleted": true
  }')

echo "Response: $UPDATE_RESPONSE"
echo ""

echo "=== All Tests Completed ==="
