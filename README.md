# Scanned Card Denoiser
## 개요
![ScannedCardDenoiser](https://github.com/user-attachments/assets/dba2763b-ab8b-4e48-aa73-fa0c912b5ed9)

**Scanned Card Denoiser는 스캔한 트레이딩 카드 및 카드다스 이미지를 몇개의 파라미터 조정으로 대량의 이미지를 손쉽게 보정하는 목적으로 개발하는 프로그램입니다.**

| Source | Result |
| :---: | :---: |
| ![11100010](https://github.com/user-attachments/assets/5c0bfa50-e7ca-45bf-b259-4ad971968e01) | ![11100010](https://github.com/user-attachments/assets/682c976a-2eb4-41f6-8a96-0b84633ba84b) |
| 1024 x 704 | 860 x 590 |

## 매뉴얼
### 1. 입출력
* Source : 처리할 원본 이미지(jpg/png/bmp) 파일, 폴더 또는 PDF를 지정합니다. 파일 선택 시 여러 개를 한 번에 선택할 수 있습니다.
* Target : 결과가 저장될 폴더입니다. 원본의 폴더 구조를 미러링해 PNG로 저장합니다.
* PDF Extract : PDF의 페이지별 임베디드 이미지를 추출해 처리 대상으로 삼습니다.
* Subfolder : 폴더 지정 시 하위 폴더까지 처리합니다.
* Overwrite : 결과 파일이 이미 있으면 다시 처리할지 결정합니다.

### 2. 이미지 보정
처리 순서 : 로드 → 디노이즈 → 밝기 → 오토 레벨 → 샤픈 → 영역 보정 → 리사이즈 → 클립

* Non-local Means Denoise : 스캔 과정에서 포함된 먼지 등의 노이즈를 줄입니다.

| Source | Result |
| :---: | :---: |
| ![Test1](https://github.com/user-attachments/assets/53aa5ede-e056-4a47-906c-8af897c2e2a4) | ![Test1](https://github.com/user-attachments/assets/4cf38ec7-b8c5-4bd3-86ad-4e9d50f4c6f9) |

* Brightness : 이미지 밝기를 조정합니다.
* Auto Level Image : 채널별 히스토그램 기반으로 이미지의 톤 보정을 수행합니다.

| Source | Result |
| :---: | :---: |
| ![11200780](https://github.com/user-attachments/assets/782132b5-5940-42aa-88bf-793322e10187) | ![11200780](https://github.com/user-attachments/assets/f024d5e8-fe9b-4e5a-a5ae-dbccc92af235) |

* Sharpen : 이미지 선명도를 올립니다.

### 3. Image Area Adjustment (영역 보정)
이미지 수평을 맞추고(deskew) 카드 영역만 잘라냅니다.

| Source | Result |
| :---: | :---: |
| ![Test1](https://github.com/user-attachments/assets/a5753f65-2e1f-40d8-968a-18a0828662ce) | ![Test1](https://github.com/user-attachments/assets/f4c49df2-bb69-42dc-a334-f5df63db153e) |

* No : 영역 보정을 하지 않습니다.
* Auto : 카드 4모서리를 자동 검출해 원근 보정(크롭)합니다.
  * 기울기 보정 → Auto Level → 어두운 영역 마스크(Otsu + Threshold 오프셋) → 모폴로지/홀 채우기 → 변당 64샘플 프로빙 + Huber 직선 피팅 → 4직선 교점 = 카드 4모서리
  * Threshold : 마스크 분리 민감도 오프셋입니다. 배경/카드 분리가 잘 안 될 때 조정합니다.
* Manual : 포인트를 직접 지정해 보정합니다.
  * `포인트 지정` 버튼으로 지정 창을 엽니다(자동 검출 결과가 초기 위치로 제공됨).
  * 코너 4점 + 변 중앙 4점을 드래그해 조정합니다. 변 중앙점을 잡으면 변 전체가 함께 이동합니다.
  * 코너의 기즈모 화살표(수직=초록, 수평=파랑)를 잡으면 해당 축으로만 이동합니다.
  * 하단 기울기 슬라이더(-90°~+90°, 0.1° 단위)로 4점을 중점 기준으로 회전할 수 있습니다.
  * `리셋` 버튼은 코너 4점을 이미지 네 끝으로 되돌립니다. 드래그 중에는 돋보기가 표시됩니다.
  * 폴더(다중 파일) 실행 시 1단계에서 파일마다 포인트를 순서대로 지정받고, 2단계에서 저장된 포인트로 일괄 처리합니다.
  * 원본 이미지가 바뀌면 지정한 포인트는 초기화됩니다.

### 4. 이미지 편집
* 사이즈 변경 : 최종 이미지 크기를 결정합니다. (4x6 / Carddass / Trading Card / Custom / Percent)
* 모서리 자르기 : 최종 이미지의 상하좌우를 지정한 픽셀만큼 자릅니다.
* 테두리 그리기 : 최종 이미지의 테두리를 검은 픽셀로 채웁니다.
* 코너 라운딩 : 최종 이미지의 코너를 라운딩화합니다.

### 5. 결과 프리뷰
* 메인 창 우측에 처리 결과 썸네일(최대 300px)이 표시되며, 보정 옵션 값을 바꾸면 자동으로 갱신됩니다(갱신 중에는 프로그레스 서클 표시).
* 썸네일에 마우스를 올리면 아래 줌 뷰에 담길 영역이 프레임으로 표시되고, 클릭하면 그 영역을 실제 해상도(1:1)로 아래 줌 뷰에 보여줍니다. 줌 위치는 옵션을 바꿔도 유지되어 품질 변화를 비교할 수 있습니다.
* `Show Result` : 현재 옵션으로 한 장을 처리해 결과 창을 띄웁니다. 원본과 결과가 겹쳐 있고, 세로 분할선을 드래그해 원본|결과를 비교할 수 있습니다(확대 슬라이더/마우스휠 줌 지원).

### 6. 실행
* Execute : 대상 전체를 순차 처리합니다. 진행률/예상 시간이 표시되며 Abort로 중단할 수 있습니다.
* 입력 경로와 보정 옵션은 자동 저장되어 다음 실행 시 복원됩니다.
