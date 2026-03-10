FROM nginx:stable-alpine

# ลบไฟล์ Config เดิมของ Nginx ออกก่อน
RUN rm /etc/nginx/conf.d/default.conf

# ก๊อปปี้ไฟล์ nginx.conf จากเครื่องเราเข้าไปในตัว Image
COPY nginx.conf /etc/nginx/nginx.conf

# หากคุณมีไฟล์ Config อื่นๆ ในโฟลเดอร์ conf.d ให้ก๊อปเข้าไปด้วย (ถ้าไม่มีข้ามบรรทัดนี้ได้)
# COPY conf.d/ /etc/nginx/conf.d/

EXPOSE 80 443
CMD ["nginx", "-g", "daemon off;"]