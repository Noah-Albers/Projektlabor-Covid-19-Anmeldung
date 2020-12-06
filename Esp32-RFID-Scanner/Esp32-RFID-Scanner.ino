/*
   Pin-Connection:
     SDA = G5
     SCK = G18
     MOSI = G23
     MISO = G19
     RST = G22
     GND = GND
     3.3V = 3.3V
*/

#include <SPI.h>
#include <MFRC522.h>
#define SS_PIN 5
#define RST_PIN 22

// Startes the RFID-Sensor
MFRC522 mfrc522(SS_PIN, RST_PIN);

void setup() {
  // Starts the serial output
  Serial.begin(9600);

  SPI.begin();
  mfrc522.PCD_Init();
}

void loop(){

  // Checks if the authentication byte got send
  if (Serial.available() && Serial.read() == 'I')
    // Sends it auth-info
    Serial.print("RFIDEspScanner");

  // Checks if no card is present or no sensor has been choosen
  if (!mfrc522.PICC_IsNewCardPresent() || !mfrc522.PICC_ReadCardSerial())
    return;

  // Iterates over all bytes from the sensor
  for (byte i = 0; i < mfrc522.uid.size; i++){
    // Outputs the data using the 4 hex blocks
    Serial.print(mfrc522.uid.uidByte[i], HEX);
  }
  // Sends the end character
  Serial.print(".");
  // Gives a little delay
  delay(500);
}
