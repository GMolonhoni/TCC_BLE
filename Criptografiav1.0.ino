/* Software Teste Esp32*/

// ------- Bibliotecas ----------- \\
#include <BLE.h>
#include <BLEUtils.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>
#include <BLEDevice.h>
#include <BLEServer.h>
#include <BLE2902.h>
#include "OneWire.h"
#include "DallasTemperature.h"
#include "mbedtls/aes.h"

// ------- Mapeamento do Hardware -----------

// -------  Constantes e Objetos -----------

#define SERVICE_UUID           "6E400001-B5A3-F393-E0A9-E50E24DCCA9E" // UART service UUID
#define DHTDATA_CHAR_UUID      "6E400003-B5A3-F393-E0A9-E50E24DCCA9E"

#define sensorsQuantity 2

// ------- Objetos e estruturas -----------

BLECharacteristic *pCharacteristic;

OneWire oneWire(22);
DallasTemperature tempSensor(&oneWire);


enum sensor_IDs
{
  sensor_temperatura = 1,
  sensor_distancia = 2
} IDs;

enum sensor_unidades
{
  celsius = 1,
  metros = 2
} unidades;

struct Data_Package {
  char caracter_inicio;
  char sensor_1_id;
  char sensor_1_valor[4];
  char sensor_1_unidade;
  char sensor_2_id;
  char sensor_2_valor[4];
  char sensor_2_unidade;
  char void_byte[2]; // 2 bytes
  char caracter_fim;
}; //avoid wrong sizes


union
{
  Data_Package data;
  byte to_send[sizeof(Data_Package)];
} mensagem;

//const unsigned char *charpointer = (const unsigned char*) &data;

// ------- Variáveis Globais -----------

bool  bleConnected = false;
bool  criptografa = true;

char * key = "1234567898765432";

char message[17];
unsigned char encryptedBuffer[16];
unsigned char decryptedBuffer[16];


//debug
float sensor_1_Valor = 0;
float sensor_1_atual = 0;
float sensor_2_Valor = 26;



// ------- Classes -----------
class MyServerCallbacks: public BLEServerCallbacks {
    void onConnect(BLEServer* pServer) {
      bleConnected = true;
      Serial.println("Conectado!");
    };

    void onDisconnect(BLEServer* pServer) {
      bleConnected = false;
      Serial.println("Desconectado!");
    }
};
class MyCallbacks: public BLECharacteristicCallbacks {
    void onWrite(BLECharacteristic *pCharacteristic) {
      std::string rxValue = pCharacteristic->getValue();
      Serial.println(rxValue[0]);

      if (rxValue.length() > 0) {
        Serial.println("*********");
        Serial.print("Received Value: ");

        for (int i = 0; i < rxValue.length(); i++)
        {
          Serial.print(rxValue[i]);
        }
        Serial.println();
        Serial.println("*********");
      }
    }
};

// ------- Configuração Inicial -----------
void setup()
{
  Serial.begin(9600);
  
  BLEsetup();
  ProtocolSetup();

  tempSensor.begin();
  
  Serial.println("Esperando um cliente se conectar...");
  Serial.println();
  
  

}
// ------- Loop Infinito -----------
void loop()
{
  if (bleConnected)
  {
    sensorsRead();
    if(value_changed())
    {
       crypto(); 
       SendBLE();   
    }
    
    //
    //SendBLE();
  }
  delay(2000);
}


// ------- Funções -----------

void sensorsRead()
{
  tempSensor.requestTemperaturesByIndex(0);
  
  sensor_1_Valor = tempSensor.getTempCByIndex(0);
  
  Serial.print("Temperatura: ");
  Serial.print(sensor_1_Valor);
  Serial.println(" C");

  convertIEEE754(sensor_1_Valor, mensagem.data.sensor_1_valor);
  convertIEEE754(sensor_2_Valor, mensagem.data.sensor_2_valor);
}

bool value_changed()
{  
  bool changed = false;
  if(sensor_1_Valor > (sensor_1_atual + 0.3) || sensor_1_Valor < (sensor_1_atual - 0.3))
  {
    sensor_1_atual = sensor_1_Valor;
    Serial.println("Valor mudou");
    changed = true;
  }
  else Serial.println("Valor nao mudou");
  return changed;
}


void SendBLE()
{
  pCharacteristic->setValue(encryptedBuffer, 16);
  pCharacteristic->notify();
}

void ProtocolSetup() //valores definidos pelo desenvolveddor
{
  mensagem.data.caracter_inicio = 2;   // caracter de inicio de texto
  
  //mensagem.data.sensor_1_id = 1;       // id do sensor 1
  mensagem.data.sensor_1_id = (char)sensor_temperatura;
  mensagem.data.sensor_1_unidade = (char)celsius; // unidade de medida do sensor 1
  
  mensagem.data.sensor_2_id = (char)sensor_distancia;       // id do sensor 2
  mensagem.data.sensor_2_unidade = (char)metros; // unidade de medida do sensor 2
  
  mensagem.data.void_byte[0] = 0xFF;   // byte "vazio" para preencher os 16bytes
  mensagem.data.void_byte[1] = 0xFF;   // byte "vazio" para preencher os 16bytes
  mensagem.data.caracter_fim = 3;      // caracter de fim de texto
}

void convertIEEE754(float number, char* bytenumber)
{
  union
  {
    float floatvalue;
    byte bytesvalue[4];
  } floatAsBytes;

  floatAsBytes.floatvalue = number;
  int j = 0;
  //Serial.print("IEEE754: ");
  for (int i = 3; i >= 0; i--)
  {
    bytenumber[j] = floatAsBytes.bytesvalue[i];
    //Serial.print(bytenumber[j], HEX);
    j++;
  }

  updatebytes();
}

void BLEsetup()
{
  // Create the BLE Device
  BLEDevice::init("MyESP32");
  // Configura o dispositivo como Servidor BLE
  BLEServer *pServer = BLEDevice::createServer();
  pServer->setCallbacks(new MyServerCallbacks());

  // Cria o servico UART
  BLEService *pService = pServer->createService(SERVICE_UUID);

  // Cria uma Característica BLE para envio dos dados
  pCharacteristic = pService->createCharacteristic(
                      DHTDATA_CHAR_UUID,
                      BLECharacteristic::PROPERTY_NOTIFY
                    );

  pCharacteristic->addDescriptor(new BLE2902());

  // Inicia o serviço
  pService->start();

  // Inicia a descoberta do ESP32
  pServer->getAdvertising()->start();
}

void updatebytes()
{
  int i = 0;
  for(char b : mensagem.to_send)
  {    
    message[i] = int(b);    
    i++;    
  }
}

void crypto()
{
  unsigned char  message_to_encrypt[16] = {0};
  Serial.println("Mensagem original:");
  for (int i = 0; i < 16; i++)
  {
    message_to_encrypt[i] = message[i];
    char str[3];
 
    sprintf(str, "%02x", (int)message_to_encrypt[i]);
    Serial.print(str);
  }
  Serial.println();
  if(criptografa)
  {
    unsigned long primeiro = micros();
    mbedtls_aes_context aes;

    mbedtls_aes_init(&aes );
    mbedtls_aes_setkey_enc( &aes, (const unsigned char*) key, strlen(key) * 8 );
    mbedtls_aes_crypt_ecb( &aes, MBEDTLS_AES_ENCRYPT,  message_to_encrypt, encryptedBuffer);
    mbedtls_aes_free( &aes );

    unsigned long segundo = micros();
    Serial.print("Tempo: ");
    Serial.println(segundo - primeiro);
    Serial.println("Mensagem criptografada:");
    
    
    for (int i = 0; i < sizeof(encryptedBuffer); i++)
    {    
      char str[3];
 
      sprintf(str, "%02x", (int)encryptedBuffer[i]);
      Serial.print(str);
    }
    Serial.println();
    decrypt(encryptedBuffer);
  }
  else
  {
    Serial.println("Mensagem nao criptografada:");
    unsigned long primeiro = micros();
    for (int i = 0; i < 16; i++) encryptedBuffer[i] = message_to_encrypt[i];

    unsigned long segundo = micros();

    Serial.print("Tempo: ");
    Serial.println(segundo - primeiro);
    
    for (int i = 0; i < 16; i++)
    {    
      char str[3];
 
      sprintf(str, "%02x", (int)encryptedBuffer[i]);
      Serial.print(str);
    }
    
  }

  
}

void decrypt(unsigned char * message)
{

  mbedtls_aes_context aes;

  mbedtls_aes_init( &aes );
  mbedtls_aes_setkey_dec( &aes, (const unsigned char*) key, strlen(key) * 8 );
  mbedtls_aes_crypt_ecb(&aes, MBEDTLS_AES_DECRYPT, (const unsigned char*)message, decryptedBuffer);
  mbedtls_aes_free( &aes );
  Serial.println("Mensagem descriptografada:");
  for (int i = 0; i < sizeof(decryptedBuffer); i++)
  {
    char str[3];
 
    sprintf(str, "%02x", (char)decryptedBuffer[i]);
    Serial.print(str);
  }
  Serial.println();
  Serial.println();
}
